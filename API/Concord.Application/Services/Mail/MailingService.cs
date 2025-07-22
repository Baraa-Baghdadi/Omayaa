using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MailKit.Security;
using MimeKit;

namespace Concord.Application.Services.Mail
{
    public class MailingService : IMailingService
    {
        private readonly IConfiguration _configuration;

        public MailingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null)
        {
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_configuration["MailSettings:Email"]),
                Subject = subject
            };
            email.To.Add(MailboxAddress.Parse(mailTo));
            var builder = new BodyBuilder();
            if (attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        file.CopyTo(ms);
                        fileBytes = ms.ToArray();

                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }

            builder.HtmlBody = body;
            email.Body = builder.ToMessageBody();
            email.From.Add(new MailboxAddress(_configuration["MailSettings:DisplayName"], _configuration["MailSettings:Email"]));
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect(_configuration["MailSettings:Host"], 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration["MailSettings:Email"], _configuration["MailSettings:Password"]);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

    }
}
