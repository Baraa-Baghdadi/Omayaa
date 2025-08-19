using Concord.Domain.Models.Notifications;

namespace Concord.Application.DTO.Notifications
{
    public class AdminNotificationDto
    {
        public Guid? EntityId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public NotificationTypeEnum Type { get; set; }
        public string? CreatedOn { get; set; } // time according user

    }
}
