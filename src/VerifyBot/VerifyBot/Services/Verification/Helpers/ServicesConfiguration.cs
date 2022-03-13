using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Verification.Configuration;

namespace VerifyBot.Services.Verification.Helpers
{
    public static class ServicesConfiguration
    {
        public static void AddVerification(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<VerificationOptions>(configuration.GetSection(VerificationOptions.Name));
            services.AddSingleton<IValidateOptions<VerificationOptions>, VerificationOptionsValidation>();
            services.AddTransient<VerificationService>();
        }
    }
}