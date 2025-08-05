using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.AdminDashboard
{
    public class DashboardCardsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NumberOfProviders { get; set; }
        public int NumberOfProducts { get; set; }
        public int NewOrdersToday { get; set; }
    }
}
