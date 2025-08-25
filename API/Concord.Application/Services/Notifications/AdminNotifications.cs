using Concord.Application.DTO.Notifications;
using Concord.Application.Extentions;
using Concord.Application.Helpers;
using Concord.Application.Services.Hub;
using Concord.Domain.Models.Identity;
using Concord.Domain.Models.Notifications;
using Concord.Domain.Models.Providers;
using Concord.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Concord.Application.Services.Notifications
{
    public class AdminNotifications : IAdminNotifications
    {
        private readonly IGenericRepository<Notification> _notificationRepo;
        private readonly IGenericRepository<Provider> _providerRepo;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BroadcastHub _hub;

        public AdminNotifications(IGenericRepository<Notification> notificationRepo, IGenericRepository<Provider> providerRepo,
            IHubContext<BroadcastHub, IHubClient> hubContext, UserManager<ApplicationUser> userManager, 
            BroadcastHub hub)
        {
            _notificationRepo = notificationRepo;
            _providerRepo = providerRepo;
            _hubContext = hubContext;
            _userManager = userManager;
            _hub = hub;
        }

        public async Task CreateOrderNotification(Guid providerId, Guid orderId, 
            string omayyaOrderId, string content, NotificationTypeEnum type)
        {
            // Get Provider:
            var provider = await _providerRepo.GetFirstOrDefault(row => row.Id == providerId);

            // title for notification:
            var titleAr = "طلب جديد";

            var notification = new Notification
            {
                EntityId = orderId,
                Title = titleAr,
                Content = content,
                IconThumbnail = "",
                IsRead = false,
                Type = type,
                TenantId = new Guid("11111111-1111-1111-1111-111111111111"),
                CreatedOn = ServiceHelper.getTimeSpam(DateTime.UtcNow)!.Value
            };

            await _notificationRepo.AddAsync(notification);

            await _notificationRepo.SaveChangesAsync();

            var allConnections = _hub.getAllConnectionsId();

            if (allConnections.Any())
            {
                var tenantId = new Guid("11111111-1111-1111-1111-111111111111");
                if (allConnections.ContainsKey(tenantId))
                {
                    var connectionsId = allConnections[tenantId];

                    if (connectionsId is not null)
                    {
                        NewCreatedOrderMsg msg = new NewCreatedOrderMsg
                        {
                            OrderId = omayyaOrderId,
                            Msg = content
                        };
                        foreach (var connectionId in connectionsId)
                        {
                            await sendMessage(connectionId, msg);
                        }
                    }
                }
            }
        }

        public async Task<int> GetCountOfUnreadingMsgAsync(ClaimsPrincipal currentUser)
        {
            // get data from token:
            var user = await _userManager.FindByEmailFromClaimsPrincipal(currentUser);
            var currentTenant = user.TenantId ?? throw new Exception("Should have tenant");

            var count = await _notificationRepo.CountAsync(row => row.TenantId == currentTenant && !row.IsRead);
            return count;

        }

        public async Task<List<Notification>> GetListOfAdminNotification(ClaimsPrincipal currentUser)
        {
            // get data from token:
            var user = await _userManager.FindByEmailFromClaimsPrincipal(currentUser);
            var currentTenant = user.TenantId ?? throw new Exception("shouldHaveTenant");

            Expression<Func<Notification, bool>> filterExpression = u => true;

            filterExpression = u => u.TenantId == currentTenant;

            // order:
            Func<IQueryable<Notification>, IOrderedQueryable<Notification>> orderByExpression = null!;

            orderByExpression = q => q.OrderByDescending(u => u.CreatedOn);

            var notifications = await _notificationRepo.GetAllWithFilterAsync(filterExpression, orderByExpression);

            return notifications.ToList();

        }

        public async Task MarkAllAsRead(ClaimsPrincipal currentUser)
        {
            // get data from token:
            var user = await _userManager.FindByEmailFromClaimsPrincipal(currentUser);
            var currentTenant = user.TenantId ?? throw new Exception("shouldHaveTenant");

            var notifications = (await _notificationRepo.GetAllWithFilterAsync(row => row.TenantId == currentTenant && !row.IsRead)).ToList();
            notifications.ForEach(notification => notification.IsRead = true);

            _notificationRepo.UpdateRange(notifications);

            await _notificationRepo.SaveChangesAsync();

        }

        #region methods

        private async Task sendMessage(string connectionId, NewCreatedOrderMsg msg)
        {
            await _hubContext.Clients.Client(connectionId).CustomerCreateOrder(msg);
        }

        private string GetRelativeDate(DateTime date)
        {
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - date.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);
            switch (delta)
            {
                case < 60:
                    return "justNow";
                case < 120:
                    return "oneMinuteAgo";
                case < 3600:
                    return string.Format("minutesAgo", ts.Minutes);
                case < 7200:
                    return "oneHourAgo";
                case < 86400:
                    return string.Format("hoursAgo", ts.Hours);
                default:
                    if (date.Date == DateTime.Now.Date)
                    {
                        return "today";
                    }
                    else if (date.Date == DateTime.Now.AddDays(-1).Date)
                    {
                        return "yesterday";
                    }
                    else
                    {
                        return date.ToString("yyyy/MM/dd");
                    }
            }
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }


        #endregion

    }
}
