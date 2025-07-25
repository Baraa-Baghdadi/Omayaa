using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// DTO for product list response with pagination
    /// </summary>
    public class GetProductsResponseDto
    {
        public List<ProductManagementDto> Products { get; set; } = new List<ProductManagementDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }

        // Additional statistics for admin dashboard
        public ProductStatisticsDto Statistics { get; set; }
    }
}
