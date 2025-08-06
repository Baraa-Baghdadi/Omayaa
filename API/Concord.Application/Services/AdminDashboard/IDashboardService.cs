using Concord.Application.DTO.AdminDashboard;
using Concord.Application.DTO.AdminDashboard.ChartJs;

namespace Concord.Application.Services.AdminDashboard
{
    public interface IDashboardService
    {
        // Dashboard Cards
        Task<DashboardCardsDto> GetDashboardCardsAsync();

        // Analytics
        Task<OrderAnalyticsDto> GetOrderAnalyticsAsync();
        Task<List<OrdersByStatusDto>> GetOrdersByStatusAsync();
        Task<List<BestSellingProductDto>> GetBestSellingProductsAsync(int take = 10);
        Task<List<BestSellingProductDto>> GetLeastSellingProductsAsync(int take = 10);
        Task<List<ProductCategoryDistributionDto>> GetProductCategoryDistributionAsync();
        Task<List<OrderTrendDto>> GetOrderTrendsAsync(string period = "daily", int days = 30);

        // Tables
        Task<List<LatestOrderDto>> GetLatestOrdersAsync(int take = 10);

        // Chart.js Compatible Data
        Task<ChartDataDto> GetBestSellingProductsChartAsync();
        Task<ChartDataDto> GetOrderCountByDayChartAsync(int days = 30);
        Task<ChartDataDto> GetRevenueTrendChartAsync(int days = 30);
        Task<PieChartDataDto> GetOrderStatusBreakdownChartAsync();
        Task<ChartDataDto> GetProductCategoryDistributionChartAsync();
    }
}
