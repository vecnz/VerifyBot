using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Storage.MySql.Configuration;

namespace VerifyBot.Services.Storage.MySql
{
    public class MySqlStorageService : IStorageService
    {
        private readonly MySqlStorageOptions _mySqlStorageOptions;
        private readonly ILogger<MySqlStorageService> _logger;
        
        public MySqlStorageService(
            IOptions<MySqlStorageOptions> storageOptions,
            ILogger<MySqlStorageService> logger)
        {
            _mySqlStorageOptions = storageOptions?.Value ?? throw new ArgumentNullException(nameof(storageOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}