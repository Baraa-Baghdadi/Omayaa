using Concord.Domain.Models.Products;
using System.ComponentModel.DataAnnotations;

namespace Concord.Domain.Models.Categories
{
    public class Category : BaseEntity
    {

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
