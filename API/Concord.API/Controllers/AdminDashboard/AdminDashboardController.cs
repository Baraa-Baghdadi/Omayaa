using Concord.Application.Services.AdminDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concord.API.Controllers.AdminDashboard
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Role-based authorization
    public class AdminDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(IDashboardService dashboardService, ILogger<AdminDashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard summary cards data
        /// </summary>
        [HttpGet("cards")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetDashboardCards()
        {
            try
            {
                var cards = await _dashboardService.GetDashboardCardsAsync();
                return Ok( cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard cards");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get comprehensive order analytics
        /// </summary>
        [HttpGet("analytics/orders")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetOrderAnalytics()
        {
            try
            {
                var analytics = await _dashboardService.GetOrderAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order analytics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get orders grouped by status
        /// </summary>
        [HttpGet("analytics/orders-by-status")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetOrdersByStatus()
        {
            try
            {
                var ordersByStatus = await _dashboardService.GetOrdersByStatusAsync();
                return Ok(ordersByStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by status");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get best selling products
        /// </summary>
        [HttpGet("analytics/best-selling-products")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetBestSellingProducts([FromQuery] int take = 10)
        {
            try
            {
                var products = await _dashboardService.GetBestSellingProductsAsync(take);
                return Ok( products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving best selling products");
                return StatusCode(500,"Internal server error");
            }
        }

        /// <summary>
        /// Get least selling products
        /// </summary>
        [HttpGet("analytics/least-selling-products")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetLeastSellingProducts([FromQuery] int take = 10)
        {
            try
            {
                var products = await _dashboardService.GetLeastSellingProductsAsync(take);
                return Ok( products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving least selling products");
                return StatusCode(500,  "Internal server error");
            }
        }

        /// <summary>
        /// Get product distribution by category
        /// </summary>
        [HttpGet("analytics/category-distribution")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetProductCategoryDistribution()
        {
            try
            {
                var distribution = await _dashboardService.GetProductCategoryDistributionAsync();
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category distribution");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order trends over time
        /// </summary>
        [HttpGet("analytics/order-trends")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetOrderTrends(
            [FromQuery] string period = "daily",
            [FromQuery] int days = 30)
        {
            try
            {
                var trends = await _dashboardService.GetOrderTrendsAsync(period, days);
                return Ok( trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order trends");
                return StatusCode(500,  "Internal server error");
            }
        }

        /// <summary>
        /// Get latest orders for dashboard table
        /// </summary>
        [HttpGet("tables/latest-orders")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetLatestOrders([FromQuery] int take = 10)
        {
            try
            {
                var orders = await _dashboardService.GetLatestOrdersAsync(take);
                return Ok( orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest orders");
                return StatusCode(500,  "Internal server error");
            }
        }

        // Chart.js Compatible Endpoints
        /// <summary>
        /// Get best selling products data formatted for Chart.js pie chart
        /// </summary>
        [HttpGet("charts/best-selling-products")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetBestSellingProductsChart()
        {
            try
            {
                var chartData = await _dashboardService.GetBestSellingProductsChartAsync();
                return Ok( chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving best selling products chart data");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order count by day formatted for Chart.js bar chart
        /// </summary>
        [HttpGet("charts/orders-by-day")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetOrderCountByDayChart([FromQuery] int days = 30)
        {
            try
            {
                var chartData = await _dashboardService.GetOrderCountByDayChartAsync(days);
                return Ok( chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by day chart data");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get revenue trends formatted for Chart.js line chart
        /// </summary>
        [HttpGet("charts/revenue-trends")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetRevenueTrendChart([FromQuery] int days = 30)
        {
            try
            {
                var chartData = await _dashboardService.GetRevenueTrendChartAsync(days);
                return Ok( chartData );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue trend chart data");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order status breakdown formatted for Chart.js doughnut chart
        /// </summary>
        [HttpGet("charts/order-status-breakdown")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetOrderStatusBreakdownChart()
        {
            try
            {
                var chartData = await _dashboardService.GetOrderStatusBreakdownChartAsync();
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order status breakdown chart data");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get product category distribution formatted for Chart.js polar area chart
        /// </summary>
        [HttpGet("charts/category-distribution")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetProductCategoryDistributionChart()
        {
            try
            {
                var chartData = await _dashboardService.GetProductCategoryDistributionChartAsync();
                return Ok( chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category distribution chart data");
                return StatusCode(500,  "Internal server error");
            }
        }
    }
}
