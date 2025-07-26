using Concord.Domain.Models.Categories;
using System.ComponentModel.DataAnnotations;

namespace Concord.Domain.Models.Products
{
    public class Product : BaseEntity
    {

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public int Price { get; set; }

        // For make offer on this product
        [Range(0, double.MaxValue, ErrorMessage = "New price must be greater than or equal to 0")]
        public int? NewPrice { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
