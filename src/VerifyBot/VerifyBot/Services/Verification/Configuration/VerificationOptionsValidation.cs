using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace VerifyBot.Services.Verification.Configuration
{
    public class VerificationOptionsValidation : IValidateOptions<VerificationOptions>
    {
        public ValidateOptionsResult Validate(string name, VerificationOptions options)
        {
            // Validate email regex.
            if (string.IsNullOrWhiteSpace(options.EmailPattern))
            {
                return ValidateOptionsResult.Fail("Missing email RegEx pattern.");
            }
            
            try
            {
                Regex.Match("", options.EmailPattern);
            }
            catch (ArgumentException)
            {
                return ValidateOptionsResult.Fail("Invalid email RegEx pattern.");
            }

            // Validate regex match group for username
            if (options.EmailUsernameMatchGroup < 0)
            {
                return ValidateOptionsResult.Fail("Email username match group cannot be less than 0.");
            }
            
            // Validate token expiry time
            if (TimeSpan.Zero.Equals(options.VerificationTokenExpiry))
            {
                return ValidateOptionsResult.Fail("Verification token expiry cannot be zero.");
            }
            
            return ValidateOptionsResult.Success;
        }
    }
}