using Concord.Application.DTO.Orders;
using Concord.Application.Extentions;
using Concord.Application.Services.Providers;
using Concord.Domain.Enums;
using Concord.Domain.Models.Identity;
using Concord.Domain.Models.Orders;
using Concord.Domain.Models.Products;
using Concord.Domain.Models.Providers;
using Concord.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Concord.Application.Services.Orders
{
    /// <summary>
    /// Service implementation for order management operations
    /// </summary>
    public class OrderManagementService : IOrderManagementService
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<OrderItem> _orderItemRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Provider> _providerRepository;
        private readonly IProviderManagementService _providerManagementService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderManagementService(
            IGenericRepository<Order> orderRepository,
            IGenericRepository<OrderItem> orderItemRepository,
            IGenericRepository<Product> productRepository,
            IGenericRepository<Provider> providerRepository,
            IProviderManagementService providerManagementService,
            UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _providerRepository = providerRepository;
            _providerManagementService = providerManagementService;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets all orders with advanced filtering, sorting and pagination
        /// </summary>
        public async Task<GetOrdersResponseDto> GetAllOrdersAsync(GetOrdersRequestDto request)
        {
            try
            {
                // Validate and set defaults for pagination
                var pageNumber = Math.Max(1, request.PageNumber);
                var pageSize = Math.Min(Math.Max(1, request.PageSize), 100);

                // Build filter expression
                Expression<Func<Order, bool>> filter = o => true;

                // Apply search filter (search in order number, customer name, phone)
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.Trim().ToLower();
                    filter = CombineWithAnd(filter, o =>
                        o.OrderNumber.ToLower().Contains(searchTerm) || o.Provider.ProviderName.ToLower().Contains(searchTerm));
                }

                // Apply provider filter
                if (request.ProviderId.HasValue)
                {
                    filter = CombineWithAnd(filter, o => o.ProviderId == request.ProviderId.Value);
                }

                // Apply date range filter
                if (request.StartDate.HasValue)
                {
                    filter = CombineWithAnd(filter, o => o.OrderDate >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    var endDate = request.EndDate.Value.Date.AddDays(1); // Include full end date
                    filter = CombineWithAnd(filter, o => o.OrderDate < endDate);
                }

                // Apply sorting
                Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = GetOrderByExpression(request.SortBy, request.SortDirection);

                // Determine includes
                var includes = "Provider";
                if (request.IncludeOrderItems)
                {
                    includes += ",OrderItems,OrderItems.Product";
                }

                // Get filtered orders with pagination
                var orders = await _orderRepository.GetAllWithFilterAsync(
                    filter: filter,
                    orderBy: orderBy,
                    includeProperties: includes,
                    pageNumber: pageNumber,
                    pageSize: pageSize
                );

                // Get total count for pagination
                var totalCount = await _orderRepository.CountAsync(filter);

                // Calculate pagination info
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return new GetOrdersResponseDto
                {
                    Orders = orders.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving orders: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a specific order by ID with all details
        /// </summary>
        public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetFirstOrDefault(
                    o => o.Id == orderId,
                    "Provider,OrderItems,OrderItems.Product"
                );

                return order != null ? MapToDto(order) : null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving order {orderId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a specific order by order number
        /// </summary>
        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            try
            {
                var order = await _orderRepository.GetFirstOrDefault(
                    o => o.OrderNumber == orderNumber,
                    "Provider,OrderItems,OrderItems.Product"
                );

                return order != null ? MapToDto(order) : null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving order {orderNumber}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new order with order items from admin side
        /// </summary>
        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            try
            {
                // Validate provider exists
                var provider = await _providerRepository.GetByIdAsync(createOrderDto.ProviderId);
                if (provider == null)
                {
                    throw new ArgumentException("Provider not found");
                }

                // Validate products and calculate totals
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var itemDto in createOrderDto.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
                    }

                    if (!product.IsActive)
                    {
                        throw new ArgumentException($"Product '{product.Name}' is not active");
                    }

                    var unitPrice = product.NewPrice ?? product.Price;
                    var itemTotal = unitPrice * itemDto.Quantity;
                    totalAmount += itemTotal;

                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = itemDto.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = itemTotal,
                        Notes = itemDto.Notes,
                        CreatedAt = DateTime.UtcNow
                    };

                    orderItems.Add(orderItem);
                }

                // Calculate final amount
                var finalAmount = totalAmount - createOrderDto.DiscountAmount ?? 0;
                if (finalAmount < 0)
                {
                    throw new ArgumentException("Discount amount cannot be greater than total amount");
                }

                // Generate unique order number
                var orderNumber = await GenerateOrderNumberAsync();

                // Create order
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = orderNumber,
                    ProviderId = createOrderDto.ProviderId,
                    TotalAmount = totalAmount,
                    DiscountAmount = createOrderDto.DiscountAmount ?? 0,
                    FinalAmount = finalAmount,
                    Notes = createOrderDto.Notes,
                    OrderDate = DateTime.UtcNow,
                    DeliveryDate = createOrderDto.DeliveryDate,
                    CreatedAt = DateTime.UtcNow,
                    OrderItems = orderItems
                };

                // Set order reference for items
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                }

                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveChangesAsync();

                // Return the created order with all details
                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error creating order: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new order with order items from provider side
        /// </summary>
        /// <param name="createOrderDto">Order creation data with items</param>
        /// <returns>Created order with generated order number</returns>
        public async Task<OrderDto> CreateNewOrderAsync(ClaimsPrincipal currentUser,CreateProviderOrder createOrderDto)
        {
            try
            {
                // get data from token:
                var user = await _userManager.FindByEmailFromClaimsPrincipal(currentUser);
                var currentTenant = user.TenantId ?? Guid.Empty;
                if (currentTenant == Guid.Empty) throw new ArgumentException("You should login");

                // get provider data:
                var provider = await _providerManagementService.GetProviderByTenantIdAsync(currentTenant);

                if (provider == null)
                {
                    throw new ArgumentException("Provider not found");
                }

                // Validate products and calculate totals
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var itemDto in createOrderDto.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
                    }

                    if (!product.IsActive)
                    {
                        throw new ArgumentException($"Product '{product.Name}' is not active");
                    }

                    var unitPrice = product.NewPrice ?? product.Price;
                    var itemTotal = unitPrice * itemDto.Quantity;
                    totalAmount += itemTotal;

                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = itemDto.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = itemTotal,
                        Notes = itemDto.Notes,
                        CreatedAt = DateTime.UtcNow
                    };

                    orderItems.Add(orderItem);
                }

                // Calculate final amount
                var finalAmount = totalAmount;
                if (finalAmount < 0)
                {
                    throw new ArgumentException("Discount amount cannot be greater than total amount");
                }

                // Generate unique order number
                var orderNumber = await GenerateOrderNumberAsync();

                // Create order
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = orderNumber,
                    ProviderId = provider.Id,
                    TotalAmount = totalAmount,
                    DiscountAmount = 0,
                    FinalAmount = finalAmount,
                    OrderDate = DateTime.UtcNow,
                    DeliveryDate = createOrderDto.DeliveryDate ?? null,
                    CreatedAt = DateTime.UtcNow,
                    OrderItems = orderItems
                };

                // Set order reference for items
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                }

                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveChangesAsync();

                // Return the created order with all details
                return await GetOrderByIdAsync(order.Id);
            
            }
            
            catch (Exception ex)
            {
                throw new ApplicationException($"Error creating order: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing order and its items
        /// </summary>
        public async Task<OrderDto?> UpdateOrderAsync(Guid orderId, UpdateOrderDto updateOrderDto)
        {
            try
            {
                var order = await _orderRepository.GetFirstOrDefault(
                    o => o.Id == orderId,
                    "OrderItems"
                );

                if (order == null)
                {
                    return null;
                }

                // Update order basic info
                order.DiscountAmount = updateOrderDto.DiscountAmount;
                order.Notes = updateOrderDto.Notes;
                order.DeliveryDate = updateOrderDto.DeliveryDate;
                order.UpdatedAt = DateTime.UtcNow;

                // Handle order items updates
                var existingItems = order.OrderItems.ToList();
                var updatedItemIds = updateOrderDto.OrderItems.Where(i => i.Id.HasValue).Select(i => i.Id.Value).ToList();

                // Remove items not in update list
                var itemsToRemove = existingItems.Where(ei => !updatedItemIds.Contains(ei.Id)).ToList();
                foreach (var item in itemsToRemove)
                {
                    order.OrderItems.Remove(item);
                }

                decimal totalAmount = 0;

                // Update existing items and add new ones
                foreach (var itemDto in updateOrderDto.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
                    }

                    if (!product.IsActive)
                    {
                        throw new ArgumentException($"Product '{product.Name}' is not active");
                    }

                    var unitPrice = product.NewPrice ?? product.Price;
                    var itemTotal = unitPrice * itemDto.Quantity;
                    totalAmount += itemTotal;

                    if (itemDto.Id.HasValue)
                    {
                        // Update existing item
                        var existingItem = order.OrderItems.FirstOrDefault(oi => oi.Id == itemDto.Id.Value);
                        if (existingItem != null)
                        {
                            existingItem.ProductId = product.Id;
                            existingItem.ProductName = product.Name;
                            existingItem.Quantity = itemDto.Quantity;
                            existingItem.UnitPrice = unitPrice;
                            existingItem.TotalPrice = itemTotal;
                            existingItem.Notes = itemDto.Notes;
                        }
                    }
                    else
                    {
                        // Add new item
                        var newItem = new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Quantity = itemDto.Quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = itemTotal,
                            Notes = itemDto.Notes,
                            CreatedAt = DateTime.UtcNow
                        };
                        order.OrderItems.Add(newItem);
                    }
                }

                // Update totals
                order.TotalAmount = totalAmount;
                order.FinalAmount = totalAmount - order.DiscountAmount;

                if (order.FinalAmount < 0)
                {
                    throw new ArgumentException("Discount amount cannot be greater than total amount");
                }

                _orderRepository.Update(order);
                await _orderRepository.SaveChangesAsync();

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating order: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates order status (Admin Side)
        /// </summary>
        /// <param name="orderId">Order ID to update</param>
        /// <param name="newStatus">new order status</param>
        /// <returns> boolean </returns>
        public async Task<bool> UpdateOrderStatus(UpdateOrderStatus input)
        {
            try
            {
                var order = await _orderRepository.GetFirstOrDefault(o => o.Id == input.OrderId);

                if (order == null)
                {
                    return false;
                }

                order.Status = input.NewStatus;
                _orderRepository.Update(order);
                await _orderRepository.SaveChangesAsync();
                return true;

            }
            catch (Exception ex) {
                throw new ApplicationException($"Error updating order: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes an order and all its items
        /// </summary>
        public async Task<bool> DeleteOrderAsync(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetFirstOrDefault(o => o.Id == orderId, "OrderItems");
                if (order == null)
                {
                    return false;
                }

                _orderRepository.Delete(order);
                await _orderRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting order: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets order statistics for admin dashboard
        /// </summary>
        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync()
        {
            try
            {
                var allOrders = await _orderRepository.GetAllAsync();
                var orders = allOrders.ToList();

                var totalOrders = orders.Count;
                var totalRevenue = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.FinalAmount);

                // Current month revenue
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var monthlyRevenue = orders
                    .Where(o =>
                               o.OrderDate.Month == currentMonth &&
                               o.OrderDate.Year == currentYear && o.Status == OrderStatus.Completed)
                    .Sum(o => o.FinalAmount);

                var averageOrderValue = totalOrders > 0 ? totalRevenue / orders.Where(o => o.Status == OrderStatus.Completed).Count() : 0;

                // Monthly revenue data for last 12 months
                var monthlyData = new List<MonthlyRevenueDto>();
                for (int i = 11; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.AddMonths(-i);
                    var monthOrders = orders.Where(o =>  o.OrderDate.Month == date.Month &&
                                                       o.OrderDate.Year == date.Year);

                    monthlyData.Add(new MonthlyRevenueDto
                    {
                        Month = date.ToString("MMM yyyy"),
                        Revenue = monthOrders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.FinalAmount),
                        OrderCount = monthOrders.Count()
                    });
                }

                return new OrderStatisticsDto
                {
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    AverageOrderValue = averageOrderValue,
                    MonthlyRevenueData = monthlyData
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error getting order statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets orders for a specific provider
        /// </summary>
        public async Task<GetOrdersResponseDto> GetOrdersByProviderAsync(Guid providerId, GetOrdersRequestDto request)
        {
            request.ProviderId = providerId;
            return await GetAllOrdersAsync(request);
        }

        /// <summary>
        /// Generates a unique order number
        /// </summary>
        public async Task<string> GenerateOrderNumberAsync()
        {
            var currentDate = DateTime.UtcNow;
            var datePrefix = currentDate.ToString("yyyyMMdd");

            // Find the last order number for today
            var todayOrders = await _orderRepository.GetAllAsync(
                predicate: o => o.OrderNumber.StartsWith(datePrefix)
            );

            var lastOrderNumber = todayOrders
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefault()?.OrderNumber;

            int sequence = 1;
            if (!string.IsNullOrEmpty(lastOrderNumber) && lastOrderNumber.Length >= 12)
            {
                if (int.TryParse(lastOrderNumber.Substring(8, 4), out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }

            return $"{datePrefix}{sequence:D4}";
        }

        /// <summary>
        /// Checks if order number exists
        /// </summary>
        public async Task<bool> OrderNumberExistsAsync(string orderNumber, Guid? excludeId = null)
        {
            Expression<Func<Order, bool>> filter = o => o.OrderNumber == orderNumber;

            if (excludeId.HasValue)
            {
                filter = CombineWithAnd(filter, o => o.Id != excludeId.Value);
            }

            return await _orderRepository.ExistsAsync(filter);
        }

        #region Private Helper Methods

        private static Expression<Func<T, bool>> CombineWithAnd<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T), "x");

            var left = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter).Visit(expr1.Body);
            var right = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter).Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left!, right!), parameter);
        }

        private Func<IQueryable<Order>, IOrderedQueryable<Order>> GetOrderByExpression(string sortBy, string sortDirection)
        {
            var ascending = sortDirection?.ToLower() == "asc";

            return sortBy?.ToLower() switch
            {
                "totalamount" => ascending
                    ? q => q.OrderBy(o => o.TotalAmount)
                    : q => q.OrderByDescending(o => o.TotalAmount),
                "ordernumber" => ascending
                    ? q => q.OrderBy(o => o.OrderNumber)
                    : q => q.OrderByDescending(o => o.OrderNumber),
                _ => ascending
                    ? q => q.OrderBy(o => o.OrderDate)
                    : q => q.OrderByDescending(o => o.OrderDate)
            };
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                ProviderId = order.ProviderId,
                ProviderName = order.Provider?.ProviderName ?? "Unknown",
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                Notes = order.Notes,
                Status = order.Status,
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems?.Select(MapToDto).ToList() ?? new List<OrderItemDto>(),
                TotalItems = order.OrderItems?.Sum(oi => oi.Quantity) ?? 0
            };
        }

        private OrderItemDto MapToDto(OrderItem orderItem)
        {
            return new OrderItemDto
            {
                Id = orderItem.Id,
                OrderId = orderItem.OrderId,
                ProductId = orderItem.ProductId,
                ProductName = orderItem.ProductName,
                Quantity = orderItem.Quantity,
                UnitPrice = orderItem.UnitPrice,
                TotalPrice = orderItem.TotalPrice,
                Notes = orderItem.Notes,
                CreatedAt = orderItem.CreatedAt
            };
        }

        #endregion

    }

    // Helper class for expression combination
    public class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression Visit(Expression node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}
