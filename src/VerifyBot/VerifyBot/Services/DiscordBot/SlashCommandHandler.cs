using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerifyBot.Services.DiscordBot.Commands;

namespace VerifyBot.Services.DiscordBot
{
    public class SlashCommandHandler
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger<SlashCommandHandler> _logger;
        private readonly IEnumerable<ICommand> _commands;
        private readonly Dictionary<ulong, ICommand> _registeredCommands = new Dictionary<ulong, ICommand>(); // I am aware that this is still mutable.
        
        public SlashCommandHandler(
            DiscordSocketClient discordClient,
            ILogger<SlashCommandHandler> logger,
            IEnumerable<ICommand> commands)
        {
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public void Register()
        {
            _logger.LogDebug("Hooking slash command events...");
            _discordClient.Ready += DiscordClientOnReady;
            _discordClient.SlashCommandExecuted += DiscordClientOnSlashCommandExecuted;
        }
        
        private async Task DiscordClientOnReady()
        {
            try
            {
                _logger.LogDebug("Registering slash commands with Discord.");
                _registeredCommands.Clear();
                foreach (var command in _commands)
                {
                    SocketApplicationCommand result = await _discordClient.CreateGlobalApplicationCommandAsync(command.Build());
                    _registeredCommands.Add(result.Id, command);
                }
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
        
        private Task DiscordClientOnSlashCommandExecuted(ISlashCommandInteraction command)
        {
            _logger.LogDebug("Slash command received {command}", command.Data.Name);
            try
            {
                if (!_registeredCommands.TryGetValue(command.Data.Id, out ICommand commandModule))
                {
                    _logger.LogError("Unhandled command executed {name} {id}", command.Data.Name, command.Data.Id);
                    return Task.CompletedTask;
                }

                commandModule.Execute(command);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception was thrown while running slash command {name} {id}", command.Data.Name, command.Data.Id, e);
            }
            return Task.CompletedTask;
        }
    }
}