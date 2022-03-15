using System;
using System.Security.Cryptography;
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

        public enum EmailResult
        {
            Success,
            InvalidEmail,
            Failure
        }
        
        public VerificationService(
            IStorageService storageService,
            IOptions<VerificationOptions> verificationOptions,
            ILogger<VerificationService> logger)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _verificationOptions = verificationOptions?.Value ?? throw new ArgumentNullException(nameof(verificationOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EmailResult> StartVerificationAsync(ulong userId, string email)
        {
            if (!IsEmailValid(email, out string username))
            {
                return EmailResult.InvalidEmail;
            }

            string token = await CreateVerificationTokenAsync(userId, username);
            // TODO: send email here
            // Also need to check for duplicate verifications and stuff, but that might need to be done after verification succeeds?
            // Do not want people to be able to unverify someone else if they have their username.
            // Also dont want to leak that the username has an associated account until ownership is proven.
            return EmailResult.Success;
        }

        private async Task<string> CreateVerificationTokenAsync(ulong userId, string username)
        {
            // Generate random Base32 token for user to verify with.
            byte[] tokenBuffer = new byte[RandomTokenLength];
            new RNGCryptoServiceProvider().GetBytes(tokenBuffer);
            string token = "$" + Base32.ToString(tokenBuffer);

            await _storageService.AddPendingVerificationAsync(userId, token, username);
            
            return token;
        }
        
        /// <summary>
        /// Checks if a uni username is valid
        /// </summary>
        public bool IsEmailValid(string email, out string username)
        {
            Match match = new Regex(_verificationOptions.EmailPattern, RegexOptions.IgnoreCase).Match(email);
            if (!match.Success)
            {
                username = string.Empty;
                return false;
            }

            username = match.Groups[_verificationOptions.EmailUsernameMatchGroup].Value.ToLower();
            return true;
        }
    }
}