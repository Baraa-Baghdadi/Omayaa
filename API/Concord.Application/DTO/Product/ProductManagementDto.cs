using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// DTO for product management operations (Admin view)
    /// </summary>
    public class ProductManagementDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public decimal? NewPrice { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Category information
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Additional admin information
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
