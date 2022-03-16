using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using VerifyBot.Services.Translation;
using VerifyBot.Services.Verification;

namespace VerifyBot.Services.DiscordBot.Commands
{
    public class VerifyCommand : ICommand
    {
        private readonly VerificationService _verificationService;
        private readonly ITranslator _translator;
        
        public VerifyCommand(VerificationService verificationService, ITranslator translator)
        {
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public string Name => "Verify";

        public SlashCommandProperties Build()
        {
            return new SlashCommandBuilder()
                .WithName(_translator.T("VERIFY_COMMAND_NAME"))
                .WithDescription(_translator.T("VERIFY_COMMAND_DESCRIPTION"))
                .AddOption(
                    _translator.T("VERIFY_COMMAND_EMAIL_TOKEN_OPTION_NAME"),
                    ApplicationCommandOptionType.String,
                    _translator.T("VERIFY_COMMAND_EMAIL_TOKEN_OPTION_DESCRIPTION"),
                    true)
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
                        await command.RespondAsync(_translator.T("VERIFY_COMMAND_FINISH_FAIL"), ephemeral: true);
                        break;
                    case VerificationService.FinishVerificationResult.InvalidToken:
                        await command.RespondAsync(_translator.T("VERIFY_COMMAND_FINISH_INVALID_TOKEN"), ephemeral: true);
                        break;
                    case VerificationService.FinishVerificationResult.TokenExpired:
                        await command.RespondAsync(_translator.T("VERIFY_COMMAND_FINISH_EXPIRED_TOKEN"), ephemeral: true);
                        break;
                    case VerificationService.FinishVerificationResult.Success:
                        await command.RespondAsync(_translator.T("VERIFY_COMMAND_FINISH_SUCCESS"), ephemeral: true);
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
                        await command.RespondAsync(_translator.T("VERIFY_COMMAND_START_FAIL"), ephemeral: true);
                        break;
                    case VerificationService.StartVerificationResult.InvalidEmail:
                        await command.RespondAsync(_translator.T("VERIFY_COMMAND_START_INVALID_EMAIL"), ephemeral: true);
                        break;
                    case VerificationService.StartVerificationResult.AlreadyVerified:
                        await command.RespondAsync(_translator.T("VERIFY_COMMAND_START_ALREADY_VERIFIED"), ephemeral: true);
                        break;
                    case VerificationService.StartVerificationResult.Success:
                        await command.RespondAsync(_translator.T("VERIFY_COMMAND_START_SUCCESS"), ephemeral: true);
                        break;
                    default: // Should never hit this
                        throw new ArgumentOutOfRangeException($"Unhandled {nameof(VerificationService.StartVerificationResult)} case. " + result.ToString());
                }
            }
        }
    }
}