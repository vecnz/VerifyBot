using System;
using System.IO;
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
using VerifyBot.Services.Storage.MySql.Models;

namespace VerifyBot.Services.Storage.MySql
{
    public class MySqlStorageService : IStorageService
    {
        private const string UserTable = "user";
        private const string UsernameRecordTable = "username_record";
        private const string PendingVerificationTable = "pending_verification";

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

        public async Task AddPendingVerificationAsync(string username)
        {
            var result = await getAndSetUsernameRecord(username);
            
            Console.WriteLine("test");
            // await createUserIfNotExistAsync(pendingVerification.UserId);
            //
            // using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            // await con.OpenAsync();
            // using var transaction = await con.BeginTransactionAsync();
            //
            // await con.ExecuteAsync(
            //     $@"INSERT INTO `{PendingVerificationTable}`
            //             (`user_id`, `username_record_id`, `token`, `creation_time`)
            //             VALUES (@userId, @usernameId, @token, @time);
            //             SELECT LAST_INSERT_ID();",
            //     new
            //     {
            //         userId = pendingVerification.UserId,
            //         usernameId = usernameRecordId,
            //         token = pendingVerification.Token,
            //         time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            //     });
            //
            // transaction.Commit();
        }

        private async Task createUserIfNotExistAsync(ulong userId)
        {
            using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
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
            username = username.ToLower();
            _logger.LogDebug("Waiting for username record lock...");
            await _usernameRecordLock.WaitAsync();
            _logger.LogDebug("Lock aquired.");
            using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            try
            {
                await con.OpenAsync();
                //await con.ExecuteAsync($"LOCK TABLES `{UsernameRecordTable}` WRITE;");

                var usernameRecords = await con.GetAllAsync<UsernameRecord>();
                
                // Search for existing username records.
                foreach (var usernameRecord in usernameRecords)
                {
                    if (await isUsernameMatchAsync(usernameRecord, username))
                    {
                        _logger.LogDebug("Match found: {id}", usernameRecord.id);
                        return usernameRecord;
                    }
                }
                
                _logger.LogDebug("No existing username found. Creating new record...");
                var createdUsernameRecord = await encryptUsernameAsync(username);
                _logger.LogDebug("Inserting username record...");
                createdUsernameRecord.id = await con.InsertAsync(createdUsernameRecord);
                
                //await con.ExecuteAsync("UNLOCK TABLES;");
                _logger.LogDebug("Insert complete.");
                return createdUsernameRecord;
            }
            finally
            {
                await con.CloseAsync();
                _usernameRecordLock.Release();
            }
        }

        private async Task<bool> isUsernameMatchAsync(UsernameRecord usernameRecord, string username)
        {
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            
            // Combine username, salt and pepper for hashing.
            byte[] hashInput = new byte[usernameBytes.Length + usernameRecord.username_salt.Length + _usernameHashPepper.Length];
            usernameBytes.CopyTo(hashInput, 0);
            usernameRecord.username_salt.CopyTo(hashInput, usernameBytes.Length);
            _usernameHashPepper.CopyTo(hashInput, usernameBytes.Length + usernameRecord.username_salt.Length);

            // Hash username with the salt
            byte[] hash;
            using (SHA512 sha = SHA512.Create())
                hash = sha.ComputeHash(hashInput);

            return Enumerable.SequenceEqual(usernameRecord.username_hash, hash);
        }
        
        private async Task<UsernameRecord> encryptUsernameAsync(string username)
        {
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