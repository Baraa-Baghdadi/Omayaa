using System.ComponentModel.DataAnnotations;

namespace Concord.Domain.Models
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

    }
}
