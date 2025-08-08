using Concord.Application.DTO.AdminDashboard;
using Concord.Application.DTO.AdminDashboard.ChartJs;
using Concord.Domain.Enums;
using Concord.Domain.Models.Categories;
using Concord.Domain.Models.Orders;
using Concord.Domain.Models.Products;
using Concord.Domain.Models.Providers;
using Concord.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Concord.Application.Services.AdminDashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<OrderItem> _orderItemRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Provider> _providerRepository;
        private readonly IGenericRepository<Category> _categoryRepository;

        public DashboardService(
            IGenericRepository<Order> orderRepository,
            IGenericRepository<OrderItem> orderItemRepository,
            IGenericRepository<Product> productRepository,
            IGenericRepository<Provider> providerRepository,
            IGenericRepository<Category> categoryRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _providerRepository = providerRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<DashboardCardsDto> GetDashboardCardsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var totalOrders = await _orderRepository.CountAsync();
            var totalRevenue =  _orderRepository.Query()
                .Where(o => o.Status == OrderStatus.Completed)
                .Sum(o => o.FinalAmount);

            var numberOfProviders = await _providerRepository.CountAsync();
            var numberOfProducts = await _productRepository.CountAsync(p => p.IsActive);

            var newOrdersToday = await _orderRepository.CountAsync(o =>
                o.OrderDate.Date == today);

            return new DashboardCardsDto
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                NumberOfProviders = numberOfProviders,
                NumberOfProducts = numberOfProducts,
                NewOrdersToday = newOrdersToday
            };
        }

        public async Task<OrderAnalyticsDto> GetOrderAnalyticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var thisYear = new DateTime(today.Year, 1, 1);

            var orders = _orderRepository.Query();

            var totalOrdersDaily =  orders.Count(o => o.OrderDate.Date == today);
            var totalOrdersMonthly = orders.Count(o => o.OrderDate >= thisMonth);
            var totalOrdersYearly =  orders.Count(o => o.OrderDate >= thisYear);

            var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
            var revenueDaily =  completedOrders
                .Where(o => o.OrderDate.Date == today)
                .Sum(o => o.FinalAmount);

            var revenueMonthly = completedOrders
                .Where(o => o.OrderDate >= thisMonth)
                .Sum(o => o.FinalAmount);

            decimal avgOrderValue = 0;

            if (completedOrders.Count > 0)
            {
                avgOrderValue = completedOrders.Average(o => o.FinalAmount);
            }
            else
            {
                avgOrderValue = 0;
            }

            return new OrderAnalyticsDto
            {
                TotalOrdersDaily = totalOrdersDaily,
                TotalOrdersMonthly = totalOrdersMonthly,
                TotalOrdersYearly = totalOrdersYearly,
                TotalRevenueDaily = revenueDaily,
                TotalRevenueMonthly = revenueMonthly,
                AverageOrderValue = avgOrderValue,
            };
        }

        public async Task<List<OrdersByStatusDto>> GetOrdersByStatusAsync()
        {
            var orders = _orderRepository.Query()
                .GroupBy(o => o.Status)
                .Select(g => new OrdersByStatusDto
                {
                    Status = g.Key,
                    StatusName = g.Key.ToString(),
                    Count = g.Count()
                }).ToList();

            return orders;
        }

        public async Task<List<BestSellingProductDto>> GetBestSellingProductsAsync(int take = 10)
        {
            var bestSelling = await _orderItemRepository.Query()
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(oi => oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name, CategoryName = oi.Product.Category.Name })
                .Select(g => new BestSellingProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    CategoryName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(take)
                .ToListAsync();

            return bestSelling;
        }

        public async Task<List<BestSellingProductDto>> GetLeastSellingProductsAsync(int take = 10)
        {
            var leastSelling = await _orderItemRepository.Query()
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(oi => oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name, CategoryName = oi.Product.Category.Name })
                .Select(g => new BestSellingProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    CategoryName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderBy(p => p.TotalSold)
                .Take(take)
                .ToListAsync();

            return leastSelling;
        }

        public async Task<List<ProductCategoryDistributionDto>> GetProductCategoryDistributionAsync()
        {
            var distribution = await _productRepository.Query()
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .GroupBy(p => new { p.CategoryId, p.Category.Name })
                .Select(g => new ProductCategoryDistributionDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    ProductCount = g.Count()
                })
                .OrderByDescending(d => d.ProductCount)
                .ToListAsync();

            return distribution;
        }

        public async Task<List<OrderTrendDto>> GetOrderTrendsAsync(string period = "daily", int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var orders = _orderRepository.Query()
                .Where(o => o.OrderDate >= startDate);

            List<OrderTrendDto> trends;

            switch (period.ToLower())
            {
                case "hourly":
                    trends = await orders
                        .GroupBy(o => new {
                            Date = o.OrderDate.Date,
                            Hour = o.OrderDate.Hour
                        })
                        .Select(g => new OrderTrendDto
                        {
                            Date = g.Key.Date.AddHours(g.Key.Hour),
                            OrderCount = g.Count(),
                            Revenue = g.Where(o => o.Status == OrderStatus.Completed)
                                      .Sum(o => o.FinalAmount)
                        })
                        .OrderBy(t => t.Date)
                        .ToListAsync();
                    break;

                case "weekly":
                    trends = await orders
                        .GroupBy(o => new {
                            Year = o.OrderDate.Year,
                            Week = (o.OrderDate.DayOfYear - 1) / 7
                        })
                        .Select(g => new OrderTrendDto
                        {
                            Date = new DateTime(g.Key.Year, 1, 1).AddDays(g.Key.Week * 7),
                            OrderCount = g.Count(),
                            Revenue = g.Where(o => o.Status == OrderStatus.Completed)
                                      .Sum(o => o.FinalAmount)
                        })
                        .OrderBy(t => t.Date)
                        .ToListAsync();
                    break;

                default: // daily
                    trends = await orders
                        .GroupBy(o => o.OrderDate.Date)
                        .Select(g => new OrderTrendDto
                        {
                            Date = g.Key,
                            OrderCount = g.Count(),
                            Revenue = g.Where(o => o.Status == OrderStatus.Completed)
                                      .Sum(o => o.FinalAmount)
                        })
                        .OrderBy(t => t.Date)
                        .ToListAsync();
                    break;
            }

            return trends;
        }

        public async Task<List<LatestOrderDto>> GetLatestOrdersAsync(int take = 10)
        {
            var latestOrders = await _orderRepository.Query()
                .Include(o => o.Provider)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Take(take)
                .Select(o => new LatestOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    ProviderName = o.Provider.ProviderName,
                    FinalAmount = o.FinalAmount,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    ItemCount = o.OrderItems.Count
                })
                .ToListAsync();

            return latestOrders;
        }

        // Chart.js Compatible Methods
        public async Task<ChartDataDto> GetBestSellingProductsChartAsync()
        {
            var bestSelling = await GetBestSellingProductsAsync(10);

            return new ChartDataDto
            {
                Labels = bestSelling.Select(p => p.ProductName).ToList(),
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Total Sold",
                        Data = bestSelling.Select(p => (object)p.TotalSold).ToList(),
                        BackgroundColor = GenerateColors(bestSelling.Count),
                        BorderColor = GenerateColors(bestSelling.Count, 0.8m),
                        BorderWidth = 1
                    }
                }
            };
        }

        public async Task<ChartDataDto> GetOrderCountByDayChartAsync(int days = 30)
        {
            var trends = await GetOrderTrendsAsync("daily", days);

            return new ChartDataDto
            {
                Labels = trends.Select(t => t.Date.ToString("MMM dd")).ToList(),
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Orders",
                        Data = trends.Select(t => (object)t.OrderCount).ToList(),
                        BackgroundColor = new List<string> { "rgba(54, 162, 235, 0.2)" },
                        BorderColor = new List<string> { "rgba(54, 162, 235, 1)" },
                        BorderWidth = 2
                    }
                }
            };
        }

        public async Task<ChartDataDto> GetRevenueTrendChartAsync(int days = 30)
        {
            var trends = await GetOrderTrendsAsync("daily", days);

            return new ChartDataDto
            {
                Labels = trends.Select(t => t.Date.ToString("MMM dd")).ToList(),
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Revenue",
                        Data = trends.Select(t => (object)t.Revenue).ToList(),
                        BackgroundColor = new List<string> { "rgba(75, 192, 192, 0.2)" },
                        BorderColor = new List<string> { "rgba(75, 192, 192, 1)" },
                        BorderWidth = 2
                    }
                }
            };
        }

        public async Task<PieChartDataDto> GetOrderStatusBreakdownChartAsync()
        {
            var statusBreakdown = await GetOrdersByStatusAsync();

            return new PieChartDataDto
            {
                Labels = statusBreakdown.Select(s => s.StatusName).ToList(),
                Data = statusBreakdown.Select(s => (decimal)s.Count).ToList(),
                BackgroundColors = GenerateColors(statusBreakdown.Count)
            };
        }

        public async Task<ChartDataDto> GetProductCategoryDistributionChartAsync()
        {
            var distribution = await GetProductCategoryDistributionAsync();

            return new ChartDataDto
            {
                Labels = distribution.Select(d => d.CategoryName).ToList(),
                Datasets = new List<ChartDatasetDto>
                {
                    new ChartDatasetDto
                    {
                        Label = "Products",
                        Data = distribution.Select(d => (object)d.ProductCount).ToList(),
                        BackgroundColor = GenerateColors(distribution.Count),
                        BorderColor = GenerateColors(distribution.Count, 0.8m),
                        BorderWidth = 1
                    }
                }
            };
        }

        private List<string> GenerateColors(int count, decimal opacity = 0.2m)
        {
            var colors = new List<string>();
            var baseColors = new[]
            {
                "255, 99, 132", "54, 162, 235", "255, 205, 86", "75, 192, 192",
                "153, 102, 255", "255, 159, 64", "199, 199, 199", "83, 102, 255",
                "255, 99, 255", "99, 255, 132"
            };

            for (int i = 0; i < count; i++)
            {
                var colorIndex = i % baseColors.Length;
                colors.Add($"rgba({baseColors[colorIndex]}, {opacity})");
            }

            return colors;
        }
    }
}
