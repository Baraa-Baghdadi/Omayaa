namespace Concord.Domain.Models.Providers
{
    public class Provider : BaseEntity
    {
        public Guid TenantId { get; set; }
        public string ProviderName { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;

        public Provider(Guid id,Guid tenantId, string providerName,
            string telephone, string mobile, DateTime creationTime)
        {
            Id = id;
            TenantId = tenantId;
            ProviderName = providerName;
            Telephone = telephone;
            Mobile = mobile;
            CreationTime = creationTime;
        }

    }
}
