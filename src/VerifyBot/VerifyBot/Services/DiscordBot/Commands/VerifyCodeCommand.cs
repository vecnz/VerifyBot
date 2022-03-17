using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using VerifyBot.Services.Translation;
using VerifyBot.Services.Verification;

namespace VerifyBot.Services.DiscordBot.Commands
{
    public class VerifyCodeCommand : ICommand
    {
        private readonly VerificationService _verificationService;
        private readonly ITranslator _translator;

        public VerifyCodeCommand(VerificationService verificationService, ITranslator translator)
        {
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public string Name => "VerifyCode";

        public SlashCommandProperties Build()
        {
            return new SlashCommandBuilder()
                .WithName(_translator.T("VERIFY_CODE_COMMAND_NAME"))
                .WithDescription(_translator.T("VERIFY_CODE_COMMAND_DESCRIPTION"))
                .AddOption(
                    _translator.T("VERIFY_CODE_COMMAND_TOKEN_OPTION_NAME"),
                    ApplicationCommandOptionType.String,
                    _translator.T("VERIFY_CODE_COMMAND_TOKEN_OPTION_DESCRIPTION"),
                    true)
                .Build();
        }

        public async Task ExecuteAsync(ISlashCommandInteraction command)
        {
            string firstOption = command.Data.Options.First().Value.ToString();
            VerificationService.FinishVerificationResult result =
                await _verificationService.FinishVerificationAsync(command.User.Id, firstOption);
            switch (result)
            {
                case VerificationService.FinishVerificationResult.Failure:
                    await command.FollowupAsync(_translator.T("VERIFY_CODE_COMMAND_FAIL"), ephemeral: true);
                    break;
                case VerificationService.FinishVerificationResult.InvalidToken:
                    await command.FollowupAsync(_translator.T("VERIFY_CODE_COMMAND_INVALID_TOKEN"), ephemeral: true);
                    break;
                case VerificationService.FinishVerificationResult.TokenExpired:
                    await command.FollowupAsync(_translator.T("VERIFY_CODE_COMMAND_EXPIRED_TOKEN"), ephemeral: true);
                    break;
                case VerificationService.FinishVerificationResult.AlreadyVerified:
                    await command.FollowupAsync(_translator.T("VERIFY_CODE_COMMAND_ALREADY_VERIFIED"), ephemeral: true);
                    break;
                case VerificationService.FinishVerificationResult.Success:
                    await command.FollowupAsync(_translator.T("VERIFY_CODE_COMMAND_SUCCESS"), ephemeral: true);
                    break;
                default: // Should never hit this
                    throw new ArgumentOutOfRangeException(
                        $"Unhandled {nameof(VerificationService.FinishVerificationResult)} case. " + result.ToString());
            }
        }
    }
}