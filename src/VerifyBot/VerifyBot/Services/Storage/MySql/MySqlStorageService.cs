using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using VerifyBot.Services.Storage.MySql.Configuration;
using VerifyBot.Services.Storage.MySql.TableModels;

namespace VerifyBot.Services.Storage.MySql
{
    public class MySqlStorageService
    {
        public const string UserTable = "user";
        public const string GuildTable = "guild";
        public const string UsernameRecordTable = "username_record";
        public const string PendingVerificationTable = "pending_verification";

        private static readonly SemaphoreSlim _usernameRecordLock = new SemaphoreSlim(1, 1);
        
        private readonly MySqlStorageOptions _mySqlStorageOptions;
        private readonly ILogger<MySqlStorageService> _logger;
        private readonly X509Certificate2 _publicKeyCert;
        private readonly byte[] _usernameHashPepper;
        
        public MySqlStorageService(
            IOptions<MySqlStorageOptions> storageOptions,
            ILogger<MySqlStorageService> logger)
        {
            _mySqlStorageOptions = storageOptions?.Value ?? throw new ArgumentNullException(nameof(storageOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publicKeyCert = new X509Certificate2(_mySqlStorageOptions.UsernamePublicKeyPath);
            _usernameHashPepper = Convert.FromBase64String(_mySqlStorageOptions.UsernameHashPepperB64);
        }

        public async Task AddPendingVerificationAsync(ulong userId, string token, string username)
        {
            _logger.LogDebug("Adding pending verification to MySql database for user ID {userId}", userId);
            _logger.LogTrace($"Getting username record from MySql database");
            var usernameRecord = await getAndSetUsernameRecord(username);
            _logger.LogTrace($"Creating user record in DB if nonexistent.");
            await createUserIfNotExistAsync(userId);

            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();
            await con.InsertAsync(new PendingVerification()
            {
                user_id = userId,
                username_record_id = usernameRecord.id,
                token = token,
                creation_time =  DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
            _logger.LogDebug("Pending verification added to MySql database for user ID {userId}", userId);
        }

        public async Task<PendingVerification> GetPendingVerificationAsync(ulong userId, string token)
        {
            _logger.LogTrace("Getting pending verification for user ID {userId}", userId);
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();

            return await con.QuerySingleOrDefaultAsync<PendingVerification>(
                $@"SELECT * FROM `{PendingVerificationTable}` WHERE `user_id` = @userId AND `token` = @token",
                new
                {
                    userId,
                    token
                });
        }

        public async Task DeleteUserPendingVerificationsAsync(ulong userId)
        {
            _logger.LogTrace("Deleting all pending verifications for user ID {userId}", userId);
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();

            await con.ExecuteAsync(
                $@"DELETE FROM `{PendingVerificationTable}` WHERE `user_id` = @userId",
                new
                {
                    userId,
                });
        }
        
        public async Task DeletePendingVerificationsOlderThan(long time)
        {
            _logger.LogTrace("Deleting all pending verifications older than {time}", time);
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();

            await con.ExecuteAsync(
                $@"DELETE FROM `{PendingVerificationTable}` WHERE `creation_time` < @time",
                new
                {
                    time,
                });
        }
        
        public async Task SetUserVerifiedUsernameId(ulong userId, int? usernameRecordId)
        {
            _logger.LogTrace("Setting verified username for user ID {userId}", userId);
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();
            await con.ExecuteAsync(
                $@"UPDATE `{UserTable}` SET `username_record_id` = @usernameRecordId WHERE `id` = @userId;",
                new
                {
                    userId,
                    usernameRecordId
                });
        }

        public async Task<User> GetUserAsync(ulong userId)
        {
            _logger.LogTrace("Getting user ID {userId}", userId);
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();
            return await con.GetAsync<User>(userId);
        }

        public async Task<Guild> GetGuildAsync(ulong guildId)
        {
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();
            return await con.GetAsync<Guild>(guildId);
        }
        
        public async Task SetGuildVerifiedRoleId(ulong guildId, ulong roleId)
        {
            _logger.LogTrace("Getting guild ID {guildId}", guildId);
            await createGuildIfNotExistAsync(guildId);
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();
            await con.ExecuteAsync(
                $@"UPDATE `{GuildTable}`
                    SET `verified_role_id` = @roleId
                    WHERE `id` = @guildId;",
                new
                {
                    roleId,
                    guildId
                });
        }
        
        private async Task createUserIfNotExistAsync(ulong userId)
        {
            _logger.LogTrace("Creating user if nonexistent in DB {userId}", userId);
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();
            await con.ExecuteAsync(
                $@"INSERT IGNORE INTO `{UserTable}` 
                        (`id`, `creation_time`)
                        VALUES (@userId, @time);",
                new
                {
                    userId = userId,
                    time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
        }
        
        private async Task createGuildIfNotExistAsync(ulong guildId)
        {
            _logger.LogTrace("Creating guild if nonexistent in DB {guildId}", guildId);
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();
            await con.ExecuteAsync(
                $@"INSERT IGNORE INTO `{GuildTable}` 
                        (`id`, `creation_time`)
                        VALUES (@guildId, @time);",
                new
                {
                    guildId = guildId,
                    time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
        }

        /// <summary>
        /// Searches through the username_record table and returns any record matching
        /// the specified username. If no records are found, one is created.
        ///
        /// The get and set functions are combined in this method to ensure atomic update of the DB
        /// in order to prevent two of the same username being added.
        /// </summary>
        /// <param name="username">Username to store in the database.</param>
        /// <returns>A username record containing the encrypted</returns>
        private async Task<UsernameRecord> getAndSetUsernameRecord(string username)
        {
            _logger.LogTrace("Getting username record and creating if nonexistent in DB.");
            username = username.ToLower();
            _logger.LogTrace("Waiting for username record lock...");
            await _usernameRecordLock.WaitAsync();
            _logger.LogTrace("Lock aquired.");
            await using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            try
            {
                await con.OpenAsync();
                //await con.ExecuteAsync($"LOCK TABLES `{UsernameRecordTable}` WRITE;");

                var usernameRecords = await con.GetAllAsync<UsernameRecord>();
                
                // Search for existing username records.
                foreach (var usernameRecord in usernameRecords)
                {
                    if (isUsernameMatchAsync(usernameRecord, username))
                    {
                        _logger.LogTrace("Username record match found: {id}", usernameRecord.id);
                        return usernameRecord;
                    }
                }
                
                _logger.LogDebug("No existing username found. Creating new record...");
                var createdUsernameRecord = encryptUsernameAsync(username);
                _logger.LogTrace("Inserting username record...");
                createdUsernameRecord.id = await con.InsertAsync(createdUsernameRecord);
                
                //await con.ExecuteAsync("UNLOCK TABLES;");
                _logger.LogTrace("Insert complete.");
                return createdUsernameRecord;
            }
            finally
            {
                await con.CloseAsync();
                _usernameRecordLock.Release();
                _logger.LogTrace("Lock released.");
            }
        }

        /// <summary>
        /// Checks if a hashed username record database matches the specified username.
        /// </summary>
        /// <param name="usernameRecord">Encrypted and hashed username data record.</param>
        /// <param name="username">Plain text username.</param>
        /// <returns>True if username is a match, false if it's not.</returns>
        private bool isUsernameMatchAsync(UsernameRecord usernameRecord, string username)
        {
            _logger.LogTrace("Checking cryptographic username match between username and existing username record...");
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            
            // Combine username, salt and pepper for hashing.
            byte[] hashInput = new byte[usernameBytes.Length + usernameRecord.username_salt.Length + _usernameHashPepper.Length];
            usernameBytes.CopyTo(hashInput, 0);
            usernameRecord.username_salt.CopyTo(hashInput, usernameBytes.Length);
            _usernameHashPepper.CopyTo(hashInput, usernameBytes.Length + usernameRecord.username_salt.Length);

            // Hash username with the salt
            byte[] hash;
            using SHA512 sha = SHA512.Create();
            hash = sha.ComputeHash(hashInput);

            _logger.LogTrace("Cryptographic equality check complete.");
            return usernameRecord.username_hash.SequenceEqual(hash);
        }
        
        /// <summary>
        /// Encrypts the username using RSA for reversable encryption and also hashes the
        /// username to use for duplicate detection.
        /// </summary>
        /// <param name="username">username to encrypt and hash.</param>
        /// <returns>Encrypted/hashed result to be put into the database.</returns>
        private UsernameRecord encryptUsernameAsync(string username)
        {
            _logger.LogTrace("Encrypting username...");
            // Generate new username record.
            RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);

            // Encrypt username with RSA, used to allow username to be recovered.
            byte[] encryptedUsername;
            using (RSA rsa = _publicKeyCert.GetRSAPublicKey())
                encryptedUsername = rsa.Encrypt(usernameBytes, RSAEncryptionPadding.OaepSHA256);

            // 512 bit random salt for hash
            byte[] salt = new byte[64];
            rng.GetBytes(salt);

            // Combine username, salt and pepper for hashing.
            byte[] hashInput = new byte[usernameBytes.Length + salt.Length + _usernameHashPepper.Length];
            usernameBytes.CopyTo(hashInput, 0);
            salt.CopyTo(hashInput, usernameBytes.Length);
            _usernameHashPepper.CopyTo(hashInput, usernameBytes.Length + salt.Length);

            // Hash username with the salt
            byte[] hash;
            using (SHA512 sha = SHA512.Create())
                hash = sha.ComputeHash(hashInput);

            _logger.LogTrace("Username encryption complete.");
            return new UsernameRecord()
            {
                encrypted_username = encryptedUsername,
                username_salt = salt,
                username_hash = hash,
                creation_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
    }
}