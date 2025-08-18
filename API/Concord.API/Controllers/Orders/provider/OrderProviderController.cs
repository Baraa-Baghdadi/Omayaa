using Concord.Application.DTO.Orders;
using Concord.Application.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Concord.API.Controllers.Orders.provider
{
    /// <summary>
    /// Order management controller for provider operations
    /// </summary>
    /// 

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Provider")]
    [SwaggerTag("Order Management - Provider operations for managing orders")]
    public class OrderProviderController : ControllerBase
    {
        private readonly IOrderManagementService _orderService;
        private readonly ILogger<OrderProviderController> _logger;

        public OrderProviderController(IOrderManagementService orderService, ILogger<OrderProviderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new order with order items (Provider only).
        /// </summary>
        /// <param name="createOrderDto">Order creation data with items</param>
        /// <returns>Created order with generated order number.</returns>
        /// <response code="201">Order created successfully.</response>
        /// <response code="400">Invalid order data or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have provider privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost("CreateNewOrder")]
        [SwaggerOperation(
            Summary = "Create a new order",
            Description = "Creates a new order with order items and generates a unique order number. Requires provider role."
        )]
        [ProducesResponseType(typeof(OrderDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateProviderOrder createOrderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdOrder = await _orderService.CreateNewOrderAsync(User, createOrderDto);

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
        /// Retrieves a specific order by ID with all details (provider only).
        /// </summary>
        /// <param name="orderId">Order unique identifier</param>
        /// <returns>Order details with items and provider information.</returns>
        /// <response code="200">Returns the order details successfully.</response>
        /// <response code="400">Invalid order ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have provider privileges.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("{orderId:guid}")]
        [SwaggerOperation(
            Summary = "Get order by ID",
            Description = "Retrieves a specific order with all details including items and provider information. Requires provider role."
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
                var order = await _orderService.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", orderId);
                    return NotFound($"Order with ID {orderId} not found.");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order {OrderId}", orderId);
                return StatusCode(500, "An error occurred while retrieving the order.");
            }
        }
    }
}
