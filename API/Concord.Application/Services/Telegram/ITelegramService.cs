namespace Concord.Application.Services.Telegram
{
    public interface ITelegramService
    {
        Task SendMessageToOmayyaBot(string MSG);
    }
}
