using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Email.Smtp.Configuration;

namespace VerifyBot.Services.Email.Smtp
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpEmailOptions _smtpOptions;
        private readonly ILogger<SmtpEmailService> _logger;
        
        public SmtpEmailService(IOptions<SmtpEmailOptions> smtpOptions, ILogger<SmtpEmailService> logger)
        {
            _smtpOptions = smtpOptions?.Value ?? throw new ArgumentNullException(nameof(smtpOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task SendVerificationEmailAsync(string address, string token)
        {
            try
            {
                _logger.LogDebug("Sending verification email.");
                using SmtpClient client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port);
                _logger.LogTrace("SMTP ssl {sslStatus}", _smtpOptions.UseSsl ? "Enabled" : "Disabled");
                client.EnableSsl = _smtpOptions.UseSsl;
                _logger.LogTrace("SMTP authentication {sslStatus}", _smtpOptions.UseAuthentication ? "Enabled" : "Disabled");
                if (_smtpOptions.UseAuthentication)
                {
                    client.Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password);
                }

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
            
                _logger.LogTrace("Starting SMTP send...");
                await client.SendMailAsync(message);
                _logger.LogTrace("SMTP send completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email. Message: {message}", ex.Message);
                throw ex;
            }
            
        }
    }
}