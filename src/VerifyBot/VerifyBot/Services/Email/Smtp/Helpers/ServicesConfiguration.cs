using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Email.Smtp.Configuration;

namespace VerifyBot.Services.Email.Smtp.Helpers
{
    public static class ServicesConfiguration
    {
        public static void AddSmtpEmail(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpEmailOptions>(configuration.GetSection(SmtpEmailOptions.Name));
            services.AddSingleton<IValidateOptions<SmtpEmailOptions>, SmtpEmailOptionsValidation>();
            services.AddTransient<IEmailService, SmtpEmailService>();
        }
    }
}