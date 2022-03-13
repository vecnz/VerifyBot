using System;
using Microsoft.Extensions.Logging;
using VerifyBot.Services.Storage;

namespace VerifyBot.Services.Verification
{
    public class VerificationService : IStorageService
    {
        private readonly ILogger<VerificationService> _logger;
        
        public VerificationService(ILogger<VerificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}