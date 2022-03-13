using System;
using Microsoft.Extensions.Logging;

namespace VerifyBot.Services.Verification
{
    public class VerificationService
    {
        private readonly ILogger<VerificationService> _logger;
        
        public VerificationService(ILogger<VerificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}