using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// DTO for bulk operations on products
    /// </summary>
    public class BulkProductOperationDto
    {
        [Required]
        public List<Guid> ProductIds { get; set; } = new List<Guid>();

        [Required]
        public string Operation { get; set; } // "activate", "deactivate", "delete", "updateCategory"

        // Optional parameters based on operation
        public Guid? NewCategoryId { get; set; }
        public bool? NewStatus { get; set; }
    }
}
