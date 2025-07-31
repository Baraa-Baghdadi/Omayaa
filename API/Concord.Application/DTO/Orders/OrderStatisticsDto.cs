using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Orders
{
    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenueData { get; set; } = new List<MonthlyRevenueDto>();
    }
}
