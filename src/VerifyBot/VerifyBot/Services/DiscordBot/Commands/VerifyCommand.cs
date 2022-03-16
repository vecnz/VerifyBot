using System;
using System.Linq;
using System.Threading.Tasks;
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

        public string Name => "Verify";

        public SlashCommandProperties Build()
        {
            return new SlashCommandBuilder()
                .WithName("verify")
                .WithDescription("TODO: put this into translations.")
                .AddOption("email", ApplicationCommandOptionType.String, "TODO: Translate this also", true)
                .Build();
        }

        public async Task ExecuteAsync(ISlashCommandInteraction command)
        {
            string firstOption = command.Data.Options.First().Value.ToString();
            if (VerificationService.IsVerificationToken(firstOption))
            {
                VerificationService.FinishVerificationResult result =
                    await _verificationService.FinishVerificationAsync(command.User.Id, firstOption);
                switch (result)
                {
                    case VerificationService.FinishVerificationResult.Failure:
                        await command.RespondAsync("FAIL TODO: translate this.", ephemeral: true);
                        break;
                    case VerificationService.FinishVerificationResult.InvalidToken:
                        await command.RespondAsync("INVALID TODO: translate this.", ephemeral: true);
                        break;
                    case VerificationService.FinishVerificationResult.TokenExpired:
                        await command.RespondAsync("EXPIRED TODO: translate this.", ephemeral: true);
                        break;
                    case VerificationService.FinishVerificationResult.Success:
                        await command.RespondAsync("SUCCESS TODO: translate this.", ephemeral: true);
                        break;
                    default: // Should never hit this
                        throw new ArgumentOutOfRangeException($"Unhandled {nameof(VerificationService.FinishVerificationResult)} case. " + result.ToString());
                }
            }
            else
            {
                VerificationService.StartVerificationResult result =
                    await _verificationService.StartVerificationAsync(command.User.Id, firstOption);
                switch (result)
                {
                    case VerificationService.StartVerificationResult.Failure:
                        await command.RespondAsync("FAIL TODO: translate this.", ephemeral: true);
                        break;
                    case VerificationService.StartVerificationResult.InvalidEmail:
                        await command.RespondAsync("INVALID TODO: translate this.", ephemeral: true);
                        break;
                    case VerificationService.StartVerificationResult.AlreadyVerified:
                        await command.RespondAsync("ALREADY VERIFIED TODO: translate this.", ephemeral: true);
                        break;
                    case VerificationService.StartVerificationResult.Success:
                        await command.RespondAsync("SUCCESS TODO: translate this.", ephemeral: true);
                        break;
                    default: // Should never hit this
                        throw new ArgumentOutOfRangeException($"Unhandled {nameof(VerificationService.StartVerificationResult)} case. " + result.ToString());
                }
            }
        }
    }
}