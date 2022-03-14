using System;
using System.Data;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace VerifyBot.Services.Storage.MySql.Configuration
{
    public class MySqlStorageOptionsValidation : IValidateOptions<MySqlStorageOptions>
    {
        public ValidateOptionsResult Validate(string name, MySqlStorageOptions options)
        {
            // Validate MySql connection string.
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
            catch (Exception ex)
            {
                return ValidateOptionsResult.Fail($"Failed to connect to MySql DB. {ex.Message}");
            }
            finally
            {
                if (con?.State == ConnectionState.Open) con.Close();
            }

            // Validate username public key path.
            if (string.IsNullOrWhiteSpace(options.UsernamePublicKeyPath))
            {
                return ValidateOptionsResult.Fail("Missing username public key path.");
            }
            
            if (!File.Exists(options.UsernamePublicKeyPath))
            {
                return ValidateOptionsResult.Fail($"File not found: \"{options.UsernamePublicKeyPath}\"");
            }

            try
            {
                new X509Certificate2(options.UsernamePublicKeyPath);
            }
            catch (Exception ex)
            {
                return ValidateOptionsResult.Fail("Failed to load username public key: " + ex.Message);
            }
            
            // Validate username hash pepper
            if (string.IsNullOrWhiteSpace(options.UsernameHashPepperB64))
            {
                return ValidateOptionsResult.Fail("Missing username hash pepper.");
            }
            
            if(!Convert.TryFromBase64String(options.UsernameHashPepperB64, new byte[128], out _))
            {
                return ValidateOptionsResult.Fail("Invalid Base64 username hash pepper.");
            }
            
            return ValidateOptionsResult.Success;
        }
    }
}