using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Storage.Configuration;

namespace VerifyBot.Services.Storage
{
    public class StorageService
    {
        private readonly StorageOptions _storageOptions;
        private readonly ILogger<StorageService> _logger;
        
        public StorageService(
            IOptions<StorageOptions> storageOptions,
            ILogger<StorageService> logger)
        {
            _storageOptions = storageOptions?.Value ?? throw new ArgumentNullException(nameof(storageOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}