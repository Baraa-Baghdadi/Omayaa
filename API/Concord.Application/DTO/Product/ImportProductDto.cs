using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// DTO for product import from CSV/Excel
    /// </summary>
    public class ImportProductDto
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public decimal? NewPrice { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
