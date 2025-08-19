using Concord.Application.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concord.API.Controllers.Notifications
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationِAdminController : ControllerBase
    {
        private readonly IAdminNotifications _notificationService;

        public NotificationِAdminController(IAdminNotifications notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("GetListOfProviderNotification")]
        public async Task<ActionResult> GetListOfProviderNotification()
        {
            var result =  await _notificationService.GetListOfAdminNotification(User);
            return Ok(result);
        }

        [HttpPut("MarkAllAsRead")]
        public async Task MarkAllAsRead()
        {
            await _notificationService.MarkAllAsRead(User);
        }

        [HttpGet("GetCountOfUnreadingMsgAsync")]
        public async Task<ActionResult> GetCountOfUnreadingMsgAsync()
        {
            return Ok(await _notificationService.GetCountOfUnreadingMsgAsync(User));
        }

    }
}
