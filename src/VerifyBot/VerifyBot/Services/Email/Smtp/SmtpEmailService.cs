using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Serilog;
using VerifyBot.Services.Email.Smtp.Configuration;

namespace VerifyBot.Services.Email.Smtp
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpEmailOptions _smtpOptions;

        public SmtpEmailService(IOptions<SmtpEmailOptions> smtpOptions)
        {
            _smtpOptions = smtpOptions?.Value ?? throw new ArgumentNullException(nameof(smtpOptions));
        }
        
        public async Task SendVerificationEmail(string address, string token)
        {
            using SmtpClient client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port);
            client.EnableSsl = _smtpOptions.UseSsl;

            // To and from addresses
            MailAddress from = new MailAddress(_smtpOptions.FromAddress, _smtpOptions.FromName, Encoding.UTF8);
            MailAddress to = new MailAddress(address);

            // Create message
            using MailMessage message = new MailMessage(from, to);
            message.IsBodyHtml = _smtpOptions.BodyIsHtml;
            message.Body = _smtpOptions.BodyTemplate.Replace("{token}", token);
            message.BodyEncoding = Encoding.UTF8;
            message.Subject = _smtpOptions.SubjectTemplate.Replace("{token}", token);
            message.SubjectEncoding = Encoding.UTF8;
            
            await client.SendMailAsync(message);

            // Clean up
            message.Dispose();
            client.Dispose();
        }
    }
}