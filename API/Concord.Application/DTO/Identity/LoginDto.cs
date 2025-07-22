using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Identity
{
    public class LoginDto
    {
        [Required]
        public string ProviderName { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
