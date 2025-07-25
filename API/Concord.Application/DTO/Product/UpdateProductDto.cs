using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// DTO for updating an existing product
    /// </summary>
    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 150 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public int Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "New price must be greater than or equal to 0")]
        public decimal? NewPrice { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }

        // Optional image file upload (if updating image)
        public IFormFile ImageFile { get; set; }

        // Image URL (set internally after upload or kept existing)
        public string ImageUrl { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
