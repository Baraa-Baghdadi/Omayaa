namespace Concord.Application.DTO.Identity
{
    public class UserInfo
    {
        public Guid? TenantId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }

    }
}
