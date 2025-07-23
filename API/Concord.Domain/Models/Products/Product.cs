using Concord.Domain.Models.Categories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concord.Domain.Models.Products
{
    public class Product : BaseEntity
    {

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        [Url]
        public string ImageUrl { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
