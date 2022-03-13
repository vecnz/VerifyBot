using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Storage;
using VerifyBot.Services.Verification.Configuration;
using VerifyBot.Services.Verification.Helpers;

namespace VerifyBot.Services.Verification
{
    public class VerificationService
    {
        private const int RandomTokenLength = 5; // Length in bytes. Base32 encodes 5 bytes into 8 characters.

        private readonly IStorageService _storageService;
        private readonly VerificationOptions _verificationOptions;
        private readonly ILogger<VerificationService> _logger;
        private static readonly Regex CodePattern = new Regex("^\\$[A-Z0-9]+$");

        private X509Certificate2 _publicKeyCert;
        
        public VerificationService(
            IStorageService storageService,
            IOptions<VerificationOptions> verificationOptions,
            ILogger<VerificationService> logger)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _verificationOptions = verificationOptions?.Value ?? throw new ArgumentNullException(nameof(verificationOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publicKeyCert = new X509Certificate2(_verificationOptions.PublicKeyPath);
        }

        public async Task<string> Test()
        {
            return await CreateVerificationCodeAsync(0, "user");
        }
        
        private async Task<string> CreateVerificationCodeAsync(ulong discordId, string username)
        {
            RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();

            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            byte[] encryptedUsername;
            using (RSA rsa = _publicKeyCert.GetRSAPublicKey())
                encryptedUsername = rsa.Encrypt(usernameBytes, RSAEncryptionPadding.OaepSHA256);

            byte[] tokenBuffer = new byte[RandomTokenLength];

            rng.GetBytes(tokenBuffer);
            string token = "$" + Base32.ToString(tokenBuffer);
            
            //await _storageService.AddPendingVerificationAsync(token, encryptedUsername, discordId);
            return token;
        }
    }
}