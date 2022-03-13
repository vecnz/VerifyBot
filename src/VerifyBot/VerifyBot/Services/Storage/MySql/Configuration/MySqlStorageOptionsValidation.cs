using Microsoft.Extensions.Options;

namespace VerifyBot.Services.Storage.MySql.Configuration
{
    public class MySqlStorageOptionsValidation : IValidateOptions<MySqlStorageOptions>
    {
        public ValidateOptionsResult Validate(string name, MySqlStorageOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                return ValidateOptionsResult.Fail("Missing MySql connection string.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}