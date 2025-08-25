using Concord.Domain.Models.Notifications;
using System.Security.Claims;

namespace Concord.Application.Services.Notifications
{
    public interface IAdminNotifications
    {
        // Call From provider Side:
        Task CreateOrderNotification(Guid providerId, Guid orderId,
            string omayyaOrderId, string content, NotificationTypeEnum type);

        // Call From Admin:
        Task<List<Notification>> GetListOfAdminNotification(ClaimsPrincipal currentUser);

        // Call From Admin:
        Task MarkAllAsRead(ClaimsPrincipal currentUser);

        // Call From Admin:
        Task<int> GetCountOfUnreadingMsgAsync(ClaimsPrincipal currentUser);

    }
}
