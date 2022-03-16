using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VerifyBot.Services.Translation.Helpers
{
    public static class ServicesConfiguration
    {
        public static void AddHardCodedTranslator(this IServiceCollection services)
        {
            services.AddTransient<ITranslator, HardCodedTranslator>();
        }
    }
}