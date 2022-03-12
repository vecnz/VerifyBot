using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VerifyBot.Services.DiscordBot.Configuration;

namespace VerifyBot.Services.DiscordBot.Helpers
{
    public static class ServicesConfiguration
    {
        public static void AddDiscordBot(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DiscordBotOptions>(configuration.GetSection(DiscordBotOptions.Name));
            services.AddSingleton<IValidateOptions<DiscordBotOptions>, DiscordBotOptionsValidation>();
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysFail
            }));
            services.AddSingleton<IHostedService, DiscordBot>(); // The IHostedService interface is convinient for this use since it has start and stop methods.
        }
    }
}