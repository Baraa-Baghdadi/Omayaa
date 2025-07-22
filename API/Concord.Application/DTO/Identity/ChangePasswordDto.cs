using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Identity
{
    public class ChangePasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

    }
}
