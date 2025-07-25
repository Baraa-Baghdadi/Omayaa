using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// DTO for product statistics (Admin dashboard)
    /// </summary>
    public class ProductStatisticsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int ProductsWithDiscount { get; set; }
        public decimal AveragePrice { get; set; }
        public int ProductsAddedThisMonth { get; set; }
        public int ProductsAddedThisWeek { get; set; }
        public Dictionary<string, int> ProductsByCategory { get; set; } = new Dictionary<string, int>();
    }
}
