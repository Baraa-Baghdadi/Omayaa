using Concord.Domain.Models.Products;
using System.ComponentModel.DataAnnotations;

namespace Concord.Domain.Models.Categories
{
    public class Subcategory
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
