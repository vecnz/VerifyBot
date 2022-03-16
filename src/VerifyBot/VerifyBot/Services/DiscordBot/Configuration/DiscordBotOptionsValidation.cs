using Microsoft.Extensions.Options;

namespace VerifyBot.Services.DiscordBot.Configuration
{
    public class DiscordBotOptionsValidation : IValidateOptions<DiscordBotOptions>
    {
        public ValidateOptionsResult Validate(string name, DiscordBotOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Token))
            {
                return ValidateOptionsResult.Fail("Missing Discord bot token.");
            }

            if (string.IsNullOrWhiteSpace(options.DefaultVerifiedRoleName))
            {
                return ValidateOptionsResult.Fail("Missing default verified role name.");
            }
            
            return ValidateOptionsResult.Success;
        }
    }
}