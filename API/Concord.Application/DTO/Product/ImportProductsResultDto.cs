using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// DTO for bulk import results
    /// </summary>
    public class ImportProductsResultDto
    {
        public int TotalProcessed { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<ProductManagementDto> ImportedProducts { get; set; } = new List<ProductManagementDto>();
    }
}
