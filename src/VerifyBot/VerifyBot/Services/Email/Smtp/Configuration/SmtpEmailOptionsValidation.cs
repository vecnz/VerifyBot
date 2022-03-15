using System;
using System.Data;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace VerifyBot.Services.Email.Smtp.Configuration
{
    public class SmtpEmailOptionsValidation : IValidateOptions<SmtpEmailOptions>
    {
        public ValidateOptionsResult Validate(string name, SmtpEmailOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Host))
            {
                return ValidateOptionsResult.Fail("Missing SMTP host");
            }

            if (options.Port < 0)
            {
                return ValidateOptionsResult.Fail("SMTP port cannot be negative.");
            }
        
            if (string.IsNullOrWhiteSpace(options.FromAddress))
            {
                return ValidateOptionsResult.Fail("Missing SMTP from address.");
            }
        
            if (string.IsNullOrWhiteSpace(options.FromName))
            {
                return ValidateOptionsResult.Fail("Missing SMTP from name.");
            }
        
            if (string.IsNullOrWhiteSpace(options.SubjectTemplate))
            {
                return ValidateOptionsResult.Fail("Missing SMTP subject template.");
            }
        
            if (string.IsNullOrWhiteSpace(options.BodyTemplate))
            {
                return ValidateOptionsResult.Fail("Missing SMTP body template.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}