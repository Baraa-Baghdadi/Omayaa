using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Orders
{
    public class MonthlyRevenueDto
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}
