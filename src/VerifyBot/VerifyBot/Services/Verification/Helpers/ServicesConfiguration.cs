using Microsoft.Extensions.DependencyInjection;
using VerifyBot.Services.Storage;

namespace VerifyBot.Services.Verification.Helpers
{
    public static class ServicesConfiguration
    {
        public static void AddVerification(this IServiceCollection services)
        {
            services.AddTransient<VerificationService>();
        }
    }
}