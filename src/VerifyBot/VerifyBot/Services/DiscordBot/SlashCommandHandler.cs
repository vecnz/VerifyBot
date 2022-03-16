using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerifyBot.Services.DiscordBot.Commands;
using VerifyBot.Services.Translation;

namespace VerifyBot.Services.DiscordBot
{
    public class SlashCommandHandler
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger<SlashCommandHandler> _logger;
        private readonly ITranslator _translator;
        private readonly IEnumerable<ICommand> _commands;
        private readonly Dictionary<ulong, ICommand> _registeredCommands = new Dictionary<ulong, ICommand>(); // I am aware that this is still mutable.
        
        public SlashCommandHandler(
            DiscordSocketClient discordClient,
            ILogger<SlashCommandHandler> logger,
            ITranslator translator,
            IEnumerable<ICommand> commands)
        {
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
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
                    _logger.LogTrace("Registering slash commands {command}", command.Name);
                    SocketApplicationCommand result = await _discordClient.CreateGlobalApplicationCommandAsync(command.Build());
                    _registeredCommands.Add(result.Id, command);
                    _logger.LogTrace("Successfully registered slash command {command}. Discord ID {id}", command.Name, result.Id);
                }
                _logger.LogTrace("All slash commands registered.");
                // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            }
            catch(HttpException ex)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
                _logger.LogError(ex,"Error registering slash commands with Discord {message} {json}", ex.Message, json);
            }
        }
        
        private Task DiscordClientOnSlashCommandExecuted(ISlashCommandInteraction command)
        {
            _logger.LogTrace("Slash command received {command}", command.Data.Name);
            try
            {
                if (!_registeredCommands.TryGetValue(command.Data.Id, out ICommand commandModule))
                {
                    _logger.LogError("Unhandled command executed {name} {id}", command.Data.Name, command.Data.Id);
                    return Task.CompletedTask;
                }

                RunSlashCommand(commandModule, command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception was thrown while running slash command {name} {id}", command.Data.Name, command.Data.Id);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs slash command with exception handling.
        /// </summary>
        private async void RunSlashCommand(ICommand commandModule ,ISlashCommandInteraction command)
        {
            try
            {
                _logger.LogDebug("Executing slash command module {command}", commandModule.Name);
                await commandModule.ExecuteAsync(command);
                _logger.LogTrace("Slash command module {command} executed successfully.", commandModule.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while running slash command {command}. Message: {message}", command.Data.Name, ex.Message);
                _logger.LogTrace("Sending server fail message in response to failed command.");
                await command.RespondAsync(_translator.T("SERVER_ERROR"), ephemeral: true);
            }
        }
    }
}