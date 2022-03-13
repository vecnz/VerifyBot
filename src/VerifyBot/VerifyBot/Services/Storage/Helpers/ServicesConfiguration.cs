using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Storage.Configuration;

namespace VerifyBot.Services.Storage.Helpers
{
    public static class ServicesConfiguration
    {
        public static void AddMySql(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.Name));
            services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidation>();
        }
    }
}