using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Storage.MySql.Configuration;

namespace VerifyBot.Services.Storage.MySql
{
    public class MySqlStorageService : IStorageService, IHostedService
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}