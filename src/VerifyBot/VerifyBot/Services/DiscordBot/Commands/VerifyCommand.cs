using System;
using System.Linq;
using Discord;
using VerifyBot.Services.Verification;

namespace VerifyBot.Services.DiscordBot.Commands
{
    public class VerifyCommand : ICommand
    {
        private readonly VerificationService _verificationService;
        
        public VerifyCommand(VerificationService verificationService)
        {
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
        }
        
        public SlashCommandProperties Build()
        {
            return new SlashCommandBuilder()
                .WithName("verify")
                .WithDescription("TODO: put this into translations.")
                .AddOption("email", ApplicationCommandOptionType.String, "TODO: Translate this also", true)
                .Build();
        }

        public async void Execute(ISlashCommandInteraction command)
        {
            VerificationService.EmailResult result =
                await _verificationService.StartVerificationAsync(
                    command.User.Id,
                    command.Data.Options.First().Value.ToString());
            switch (result)
            {
                case VerificationService.EmailResult.Failure:
                    await command.RespondAsync("FAIL TODO: translate this.", ephemeral: true);
                    break;
                case VerificationService.EmailResult.InvalidEmail:
                    await command.RespondAsync("INVALID TODO: translate this.", ephemeral: true);
                    break;
                case VerificationService.EmailResult.Success:
                    await command.RespondAsync("SUCCESS TODO: translate this.", ephemeral: true);
                    break;
                default: // Should never hit this
                    throw new ArgumentOutOfRangeException("Unhandled EmailResult case. " + result.ToString());
            }
        }
    }
}