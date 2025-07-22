using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Identity
{
    public class ProviderRegisterDto
    {
        [Required]
        public string ProviderName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Mobile { get; set; }

        public string? Telephone { get; set; }
       
        [Required]
        public string Address { get; set; }

    }
}
