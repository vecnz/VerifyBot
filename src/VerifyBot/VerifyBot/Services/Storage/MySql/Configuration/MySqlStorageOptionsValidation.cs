using System;
using System.Data;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace VerifyBot.Services.Storage.MySql.Configuration
{
    public class MySqlStorageOptionsValidation : IValidateOptions<MySqlStorageOptions>
    {
        public ValidateOptionsResult Validate(string name, MySqlStorageOptions options)
        {
            MySqlConnection con = null;
            try
            {
                con = new MySqlConnection(options.ConnectionString);
                con.Open();
            }
            catch (ArgumentException ex)
            {
                return ValidateOptionsResult.Fail($"Invalid MySql connection string. {ex.Message}");
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    //http://dev.mysql.com/doc/refman/5.0/en/error-messages-server.html
                    case 1042: // Unable to connect to any of the specified MySQL hosts (Check Server,Port)
                        return ValidateOptionsResult.Fail($"Failed to connect to MySql DB host. {ex.Message}");
                    case 0: // Access denied (Check DB name,username,password)
                        return ValidateOptionsResult.Fail($"Failed to connect to MySql DB. Access denied. {ex.Message}");
                    default:
                        return ValidateOptionsResult.Fail($"Failed to connect to MySql DB. {ex.Message}");
                }
            }
            finally
            {
                if (con?.State == ConnectionState.Open) con.Close();
            }

            return ValidateOptionsResult.Success;
        }
    }
}