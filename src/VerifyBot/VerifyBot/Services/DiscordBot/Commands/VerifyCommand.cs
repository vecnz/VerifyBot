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
                    _translator.T("VERIFY_COMMAND_EMAIL_OPTION_NAME"),
                    ApplicationCommandOptionType.String,
                    _translator.T("VERIFY_COMMAND_EMAIL_OPTION_DESCRIPTION"),
                    true)
                .Build();
        }

        public async Task ExecuteAsync(ISlashCommandInteraction command)
        {
            string firstOption = command.Data.Options.First().Value.ToString();
            VerificationService.StartVerificationResult result =
                await _verificationService.StartVerificationAsync(command.User.Id, firstOption);
            switch (result)
            {
                case VerificationService.StartVerificationResult.Failure:
                    await command.FollowupAsync(_translator.T("VERIFY_COMMAND_FAIL"), ephemeral: true);
                    break;
                case VerificationService.StartVerificationResult.InvalidEmail:
                    await command.FollowupAsync(_translator.T("VERIFY_COMMAND_INVALID_EMAIL"), ephemeral: true);
                    break;
                case VerificationService.StartVerificationResult.AlreadyVerified:
                    await command.FollowupAsync(_translator.T("VERIFY_COMMAND_ALREADY_VERIFIED"),
                        ephemeral: true);
                    break;
                case VerificationService.StartVerificationResult.Success:
                    await command.FollowupAsync(_translator.T("VERIFY_COMMAND_SUCCESS"), ephemeral: true);
                    break;
                default: // Should never hit this
                    throw new ArgumentOutOfRangeException(
                        $"Unhandled {nameof(VerificationService.StartVerificationResult)} case. " + result.ToString());
            }
        }
    }
}