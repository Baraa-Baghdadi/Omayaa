using Concord.Application.Services.Telegram;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concord.API.Controllers.Telegram
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController : ControllerBase
    {
        private readonly ITelegramService _telegramService;
        private readonly ILogger<TelegramController> _logger;

        public TelegramController(ITelegramService telegramService, ILogger<TelegramController> logger)
        {
            _telegramService = telegramService;
            _logger = logger;
        }

        [HttpPost("send-to-telegram")]
        public async Task<ActionResult> SendMessageToGroup(string MSG)
        {
            try
            {
                await _telegramService.SendMessageToOmayyaBot(MSG);
                return Ok(true);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending failed.");
                return BadRequest(ex.Message);
            }
        }

    }
}
