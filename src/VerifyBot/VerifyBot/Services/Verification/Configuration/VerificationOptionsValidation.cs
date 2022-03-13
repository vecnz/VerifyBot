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

            // Validate public key path.
            if (string.IsNullOrWhiteSpace(options.PublicKeyPath))
            {
                return ValidateOptionsResult.Fail("Missing public key path.");
            }
            
            if (!File.Exists(options.PublicKeyPath))
            {
                return ValidateOptionsResult.Fail($"File not found: \"{options.PublicKeyPath}\"");
            }

            try
            {
                new X509Certificate2(options.PublicKeyPath);
            }
            catch (Exception ex)
            {
                return ValidateOptionsResult.Fail("Failed to load public key: " + ex.Message);
            }
            
            return ValidateOptionsResult.Success;
        }
    }
}