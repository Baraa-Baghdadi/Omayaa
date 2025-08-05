using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.AdminDashboard
{
    public class OrderAnalyticsDto
    {
        public int TotalOrdersDaily { get; set; }
        public int TotalOrdersMonthly { get; set; }
        public int TotalOrdersYearly { get; set; }
        public decimal TotalRevenueDaily { get; set; }
        public decimal TotalRevenueMonthly { get; set; }
        public decimal AverageOrderValue { get; set; }
        public double AveragePreparationTimeHours { get; set; }
    }
}
