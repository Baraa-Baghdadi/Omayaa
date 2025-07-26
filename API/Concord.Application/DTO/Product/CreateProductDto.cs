using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// DTO for creating a new product
    /// </summary>
    public class CreateProductDto
    {
        /// <summary>
        /// Product name (required, max 150 characters)
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(150, ErrorMessage = "Product name cannot exceed 150 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Regular price in the smallest currency unit (required, must be >= 0)
        /// </summary>
        [Required(ErrorMessage = "Price is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public int Price { get; set; }

        /// <summary>
        /// Discounted price in the smallest currency unit (optional, must be >= 0 if provided)
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "New price must be greater than or equal to 0")]
        public int? NewPrice { get; set; }

        /// <summary>
        /// Product image file (optional)
        /// </summary>
        public IFormFile? Image { get; set; }

        /// <summary>
        /// Whether the product should be active (default: true)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Category identifier this product belongs to (required)
        /// </summary>
        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }
    }
}
