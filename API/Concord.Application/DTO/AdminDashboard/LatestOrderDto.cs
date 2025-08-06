using Concord.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.AdminDashboard
{
    public class LatestOrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public string ProviderName { get; set; }
        public decimal FinalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public int ItemCount { get; set; }
    }
}
