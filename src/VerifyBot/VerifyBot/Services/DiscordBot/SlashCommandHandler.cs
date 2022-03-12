using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VerifyBot.Services.DiscordBot
{
    public class SlashCommandHandler
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger<SlashCommandHandler> _logger;
        
        public SlashCommandHandler(
            DiscordSocketClient discordClient,
            ILogger<SlashCommandHandler> logger)
        {
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Register()
        {
            _logger.LogDebug("Hooking slash command events...");
            _discordClient.Ready += DiscordClientOnReady;
            _discordClient.SlashCommandExecuted += DiscordClientOnSlashCommandExecuted;
        }
        
        private async Task DiscordClientOnReady()
        {
            // Let's do our global command
            var globalCommand = new SlashCommandBuilder();
            globalCommand
                .WithName("verify")
                .WithDescription("TODO: put this into translations.")
                .AddOption("email", ApplicationCommandOptionType.String, "TODO: Translate this also", true);

            try
            {
                // With global commands we don't need the guild.
                _logger.LogDebug("Registering slash commands with Discord.");
                await _discordClient.CreateGlobalApplicationCommandAsync(globalCommand.Build());
                // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            }
            catch(HttpException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                _logger.LogError(json);
            }
        }
        
        private async Task DiscordClientOnSlashCommandExecuted(SocketSlashCommand command)
        {
            _logger.LogDebug("Slash command received {command}", command.CommandName);
            await command.RespondAsync("Hello! " + command.Data.Options.First().Value, ephemeral: true);
        }
    }
}