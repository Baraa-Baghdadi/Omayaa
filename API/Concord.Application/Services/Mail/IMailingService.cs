
using Microsoft.AspNetCore.Http;

namespace Concord.Application.Services.Mail
{
    public interface IMailingService
    {
        Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null);
    }
}
