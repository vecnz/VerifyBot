using Microsoft.Extensions.Options;

namespace VerifyBot.Services.Storage.Configuration
{
    public class StorageOptionsValidation : IValidateOptions<StorageOptions>
    {
        public ValidateOptionsResult Validate(string name, StorageOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.MySqlConnectionString))
            {
                return ValidateOptionsResult.Fail("Missing MySql connection string.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}