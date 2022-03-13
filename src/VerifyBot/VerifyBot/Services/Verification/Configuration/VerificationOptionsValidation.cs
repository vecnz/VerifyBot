using Microsoft.Extensions.Options;

namespace VerifyBot.Services.Verification.Configuration
{
    public class VerificationOptionsValidation : IValidateOptions<VerificationOptions>
    {
        public ValidateOptionsResult Validate(string name, VerificationOptions options)
        {
            return ValidateOptionsResult.Success;
        }
    }
}