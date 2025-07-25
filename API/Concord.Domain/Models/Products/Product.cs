using Concord.Domain.Models.Categories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concord.Domain.Models.Products
{
    public class Product : BaseEntity
    {

        [Required, MaxLength(150)]
        public string Name { get; set; }

        public int Price { get; set; }
        
        // for make offer on this product
        public decimal? NewPrice { get; set; }
        public string ImageUrl { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
