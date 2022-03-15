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
            
            return ValidateOptionsResult.Success;
        }
    }
}