using Concord.Domain.Models.Categories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concord.Domain.Models.Products
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Url]
        public string ImageUrl { get; set; }

        public int SubcategoryId { get; set; }
        public Subcategory Subcategory { get; set; }
    }
}
