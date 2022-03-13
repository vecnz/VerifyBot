using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Storage.MySql.Configuration;

namespace VerifyBot.Services.Storage.MySql.Helpers
{
    public static class ServicesConfiguration
    {
        public static void AddMySql(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MySqlStorageOptions>(configuration.GetSection(MySqlStorageOptions.Name));
            services.AddSingleton<IValidateOptions<MySqlStorageOptions>, MySqlStorageOptionsValidation>();
            services.AddScoped<IStorageService, MySqlStorageService>();
            services.AddScoped<IHostedService, MySqlStorageService>();
        }
    }
}