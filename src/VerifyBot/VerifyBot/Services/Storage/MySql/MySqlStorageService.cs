using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using VerifyBot.Services.Storage.Models;
using VerifyBot.Services.Storage.MySql.Configuration;

namespace VerifyBot.Services.Storage.MySql
{
    public class MySqlStorageService : IStorageService
    {
        private const string UserTable = "user";
        private const string UsernameRecordTable = "username_record";
        private const string PendingVerificationTable = "pending_verification";

        private readonly MySqlStorageOptions _mySqlStorageOptions;
        private readonly ILogger<MySqlStorageService> _logger;

        public MySqlStorageService(
            IOptions<MySqlStorageOptions> storageOptions,
            ILogger<MySqlStorageService> logger)
        {
            _mySqlStorageOptions = storageOptions?.Value ?? throw new ArgumentNullException(nameof(storageOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddPendingVerificationAsync(PendingVerification pendingVerification)
        {
            await createUserIfNotExistAsync(pendingVerification.UserId);

            using var con = new MySqlConnection(_mySqlStorageOptions.ConnectionString);
            await con.OpenAsync();
            using var transaction = await con.BeginTransactionAsync();

            int usernameRecordId = await con.QuerySingleAsync<int>(
                $@"INSERT INTO `{UsernameRecordTable}`
                        (`encryption_iv`, `encrypted_username`, `encrypted_salt`, `encrypted_hash`, `creation_time`)
                        VALUES (@iv, @encUsername, @encSalt, @encHash, @time);
                        SELECT LAST_INSERT_ID();",
                new
                {
                    iv = new byte[0],
                    encUsername = new byte[0],
                    encSalt = new byte[0],
                    encHash = new byte[0],
                    time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });

            await con.ExecuteAsync(
                $@"INSERT INTO `{PendingVerificationTable}`
                        (`user_id`, `username_record_id`, `token`, `creation_time`)
                        VALUES (@userId, @usernameId, @token, @time);
                        SELECT LAST_INSERT_ID();",
                new
                {
                    userId = pendingVerification.UserId,
                    usernameId = usernameRecordId,
                    token = pendingVerification.Token,
                    time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });

            transaction.Commit();
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
    }
}