using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VerifyBot.Services.DiscordBot.Configuration;

namespace VerifyBot.Services.DiscordBot
{
    public class DiscordBot : IHostedService
    {
        private readonly DiscordBotOptions _botOptions;
        private readonly ILogger<DiscordBot> _logger;
        
        public DiscordBot(
            IOptions<DiscordBotOptions> botOptions,
            ILogger<DiscordBot> logger)
        {
            _botOptions = botOptions?.Value ?? throw new ArgumentNullException(nameof(botOptions));
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token: {token}", _botOptions.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}