using Concord.Application.DTO.Orders;
using Concord.Application.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Concord.API.Controllers.Orders.admin
{
    /// <summary>
    /// Order management controller for administrative operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Order Management - Admin operations for managing orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderManagementService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderManagementService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all orders with advanced filtering and pagination (Admin only).
        /// </summary>
        /// <param name="request">Filter and pagination parameters</param>
        /// <returns>Paginated list of orders with detailed information.</returns>
        /// <response code="200">Returns the paginated list of orders successfully.</response>
        /// <response code="400">Invalid request parameters or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all orders with pagination and filtering",
            Description = "Retrieves all orders with advanced filtering options, sorting, and pagination. Requires admin role."
        )]
        [ProducesResponseType(typeof(GetOrdersResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetOrdersResponseDto>> GetAllOrders([FromQuery] GetOrdersRequestDto request)
        {
            try
            {
                _logger.LogInformation("Admin user requesting orders list with filters: {@Request}", request);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _orderService.GetAllOrdersAsync(request);

                if (result == null)
                {
                    return StatusCode(500, "Unable to retrieve orders.");
                }

                _logger.LogInformation("Successfully retrieved {Count} orders out of {Total} total orders",
                    result.Orders.Count, result.TotalCount);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid request parameters: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving orders");
                return StatusCode(500, "An error occurred while retrieving orders.");
            }
        }

        /// <summary>
        /// Retrieves a specific order by ID with all details (Admin only).
        /// </summary>
        /// <param name="orderId">Order unique identifier</param>
        /// <returns>Order details with items and provider information.</returns>
        /// <response code="200">Returns the order details successfully.</response>
        /// <response code="400">Invalid order ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("{orderId:guid}")]
        [SwaggerOperation(
            Summary = "Get order by ID",
            Description = "Retrieves a specific order with all details including items and provider information. Requires admin role."
        )]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid orderId)
        {
            try
            {
                _logger.LogInformation("Admin user requesting order details for ID: {OrderId}", orderId);

                var order = await _orderService.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", orderId);
                    return NotFound($"Order with ID {orderId} not found.");
                }

                _logger.LogInformation("Successfully retrieved order {OrderNumber}", order.OrderNumber);
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order {OrderId}", orderId);
                return StatusCode(500, "An error occurred while retrieving the order.");
            }
        }

        /// <summary>
        /// Retrieves a specific order by order number (Admin only).
        /// </summary>
        /// <param name="orderNumber">Order number</param>
        /// <returns>Order details with items and provider information.</returns>
        /// <response code="200">Returns the order details successfully.</response>
        /// <response code="400">Invalid order number format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("by-number/{orderNumber}")]
        [SwaggerOperation(
            Summary = "Get order by order number",
            Description = "Retrieves a specific order by its order number with all details. Requires admin role."
        )]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNumber))
                {
                    return BadRequest("Order number is required.");
                }

                _logger.LogInformation("Admin user requesting order details for number: {OrderNumber}", orderNumber);

                var order = await _orderService.GetOrderByNumberAsync(orderNumber);

                if (order == null)
                {
                    _logger.LogWarning("Order with number {OrderNumber} not found", orderNumber);
                    return NotFound($"Order with number {orderNumber} not found.");
                }

                _logger.LogInformation("Successfully retrieved order {OrderNumber}", order.OrderNumber);
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order {OrderNumber}", orderNumber);
                return StatusCode(500, "An error occurred while retrieving the order.");
            }
        }

        /// <summary>
        /// Creates a new order with order items (Admin only).
        /// </summary>
        /// <param name="createOrderDto">Order creation data with items</param>
        /// <returns>Created order with generated order number.</returns>
        /// <response code="201">Order created successfully.</response>
        /// <response code="400">Invalid order data or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new order",
            Description = "Creates a new order with order items and generates a unique order number. Requires admin role."
        )]
        [ProducesResponseType(typeof(OrderDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                _logger.LogInformation("Admin user creating new order for provider: {ProviderId}", createOrderDto.ProviderId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdOrder = await _orderService.CreateOrderAsync(createOrderDto);

                _logger.LogInformation("Successfully created order {OrderNumber} with ID: {OrderId}",
                    createdOrder.OrderNumber, createdOrder.Id);

                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { orderId = createdOrder.Id },
                    createdOrder
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid order creation data: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order");
                return StatusCode(500, "An error occurred while creating the order.");
            }
        }

        /// <summary>
        /// Updates an existing order and its items (Admin only).
        /// </summary>
        /// <param name="orderId">Order ID to update</param>
        /// <param name="updateOrderDto">Order update data</param>
        /// <returns>Updated order details.</returns>
        /// <response code="200">Order updated successfully.</response>
        /// <response code="400">Invalid order data or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="409">Order cannot be updated in current status.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPut("{orderId:guid}")]
        [SwaggerOperation(
            Summary = "Update an existing order",
            Description = "Updates an existing order and its items. Only allowed for pending or confirmed orders. Requires admin role."
        )]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OrderDto>> UpdateOrder(Guid orderId, [FromBody] UpdateOrderDto updateOrderDto)
        {
            try
            {
                _logger.LogInformation("Admin user updating order: {OrderId}", orderId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedOrder = await _orderService.UpdateOrderAsync(orderId, updateOrderDto);

                if (updatedOrder == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for update", orderId);
                    return NotFound($"Order with ID {orderId} not found.");
                }

                _logger.LogInformation("Successfully updated order {OrderNumber}", updatedOrder.OrderNumber);
                return Ok(updatedOrder);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid order update data: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Order update conflict: {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order {OrderId}", orderId);
                return StatusCode(500, "An error occurred while updating the order.");
            }
        }

        /// <summary>
        /// Deletes an order and all its items (Admin only).
        /// </summary>
        /// <param name="orderId">Order ID to delete</param>
        /// <returns>Success status.</returns>
        /// <response code="200">Order deleted successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="409">Order cannot be deleted in current status.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpDelete("{orderId:guid}")]
        [SwaggerOperation(
            Summary = "Delete an order",
            Description = "Permanently deletes an order and all its items. Only allowed for pending or cancelled orders. Requires admin role."
        )]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> DeleteOrder(Guid orderId)
        {
            try
            {
                _logger.LogInformation("Admin user deleting order: {OrderId}", orderId);

                var result = await _orderService.DeleteOrderAsync(orderId);

                if (!result)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for deletion", orderId);
                    return NotFound($"Order with ID {orderId} not found.");
                }

                _logger.LogInformation("Successfully deleted order: {OrderId}", orderId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Order deletion conflict: {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting order {OrderId}", orderId);
                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }

        /// <summary>
        /// Gets comprehensive order statistics for admin dashboard (Admin only).
        /// </summary>
        /// <returns>Order statistics including counts, revenue, and trends.</returns>
        /// <response code="200">Returns order statistics successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("statistics")]
        [SwaggerOperation(
            Summary = "Get order statistics",
            Description = "Retrieves comprehensive order statistics for admin dashboard including counts, revenue, and trends. Requires admin role."
        )]
        [ProducesResponseType(typeof(OrderStatisticsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OrderStatisticsDto>> GetOrderStatistics()
        {
            try
            {
                _logger.LogInformation("Admin user requesting order statistics");

                var statistics = await _orderService.GetOrderStatisticsAsync();

                _logger.LogInformation("Successfully retrieved order statistics: {TotalOrders} total orders, {TotalRevenue} total revenue",
                    statistics.TotalOrders, statistics.TotalRevenue);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order statistics");
                return StatusCode(500, "An error occurred while retrieving order statistics.");
            }
        }

        /// <summary>
        /// Gets orders for a specific provider (Admin only).
        /// </summary>
        /// <param name="providerId">Provider ID</param>
        /// <param name="request">Filtering and pagination parameters</param>
        /// <returns>Provider's orders with pagination.</returns>
        /// <response code="200">Returns the provider's orders successfully.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("by-provider/{providerId:guid}")]
        [SwaggerOperation(
            Summary = "Get orders by provider",
            Description = "Retrieves orders for a specific provider with filtering and pagination. Requires admin role."
        )]
        [ProducesResponseType(typeof(GetOrdersResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetOrdersResponseDto>> GetOrdersByProvider(Guid providerId, [FromQuery] GetOrdersRequestDto request)
        {
            try
            {
                _logger.LogInformation("Admin user requesting orders for provider: {ProviderId}", providerId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _orderService.GetOrdersByProviderAsync(providerId, request);

                _logger.LogInformation("Successfully retrieved {Count} orders for provider {ProviderId}",
                    result.Orders.Count, providerId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid request parameters: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving orders for provider {ProviderId}", providerId);
                return StatusCode(500, "An error occurred while retrieving orders.");
            }
        }
    }
}
