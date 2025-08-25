using System.Text;
using System.Text.Json;

namespace Concord.Application.Services.Telegram
{
    public class TelegramService : ITelegramService
    {
        private readonly HttpClient _httpClient;
        private readonly string _botToken = "7969944971:AAHjrUb0x3tCsMFQ427arjxgaX4cCV9O5L8";
        private readonly string _chatId = "-1002958417211";

        public TelegramService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task SendMessageToOmayyaBot(string MSG)
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

            var payload = new
            {
                chat_id = _chatId,
                text = MSG
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync(url, content);

        }
    }
}
