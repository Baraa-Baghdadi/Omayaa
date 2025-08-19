namespace Concord.Domain.Models.Notifications
{
    public class Notification : BaseEntity
    {
        public Guid? EntityId { get; set; } // orderId
        public bool IsRead { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NotificationTypeEnum Type { get; set; }
        public Guid? UserId { get; set; } // For mobile notification
        public Guid? TenantId { get; set; } // For provider notification
        public string? IconThumbnail { get; set; } = string.Empty;
        public decimal? CreatedOn { get; set; } // time according user

    }
}
