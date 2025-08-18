using Concord.Application.DTO.Orders;
using System.Security.Claims;

namespace Concord.Application.Services.Orders
{
    /// <summary>
    /// Service interface for order management operations (Admin only)
    /// </summary>
    public interface IOrderManagementService
    {
        /// <summary>
        /// Gets all orders with advanced filtering, sorting and pagination
        /// </summary>
        /// <param name="request">Request parameters for filtering and pagination</param>
        /// <returns>Paginated list of orders with detailed information</returns>
        Task<GetOrdersResponseDto> GetAllOrdersAsync(GetOrdersRequestDto request);

        /// <summary>
        /// Gets a specific order by ID with all details
        /// </summary>
        /// <param name="orderId">Order unique identifier</param>
        /// <returns>Order details with items</returns>
        Task<OrderDto?> GetOrderByIdAsync(Guid orderId);

        /// <summary>
        /// Gets a specific order by order number
        /// </summary>
        /// <param name="orderNumber">Order number</param>
        /// <returns>Order details with items</returns>
        Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);

        /// <summary>
        /// Creates a new order with order items from admin side
        /// </summary>
        /// <param name="createOrderDto">Order creation data with items</param>
        /// <returns>Created order with generated order number</returns>
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);

        /// <summary>
        /// Creates a new order with order items from provider side
        /// </summary>
        /// <param name="createOrderDto">Order creation data with items</param>
        /// <returns>Created order with generated order number</returns>
        Task<OrderDto> CreateNewOrderAsync(ClaimsPrincipal currentUser,CreateProviderOrder createOrderDto);

        /// <summary>
        /// Updates an existing order and its items
        /// </summary>
        /// <param name="orderId">Order ID to update</param>
        /// <param name="updateOrderDto">Order update data</param>
        /// <returns>Updated order</returns>
        Task<OrderDto?> UpdateOrderAsync(Guid orderId, UpdateOrderDto updateOrderDto);

        /// <summary>
        /// Deletes an order and all its items (Admin only)
        /// </summary>
        /// <param name="orderId">Order ID to delete</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteOrderAsync(Guid orderId);

        /// <summary>
        /// Gets order statistics for admin dashboard
        /// </summary>
        /// <returns>Comprehensive order statistics</returns>
        Task<OrderStatisticsDto> GetOrderStatisticsAsync();

        /// <summary>
        /// Gets orders for a specific provider
        /// </summary>
        /// <param name="providerId">Provider ID</param>
        /// <param name="request">Filtering and pagination parameters</param>
        /// <returns>Provider's orders</returns>
        Task<GetOrdersResponseDto> GetOrdersByProviderAsync(Guid providerId, GetOrdersRequestDto request);

        /// <summary>
        /// Generates a unique order number
        /// </summary>
        /// <returns>Unique order number</returns>
        Task<string> GenerateOrderNumberAsync();

        /// <summary>
        /// Checks if order number exists
        /// </summary>
        /// <param name="orderNumber">Order number to check</param>
        /// <param name="excludeId">Order ID to exclude from check (for updates)</param>
        /// <returns>True if order number exists</returns>
        Task<bool> OrderNumberExistsAsync(string orderNumber, Guid? excludeId = null);

    }
}
