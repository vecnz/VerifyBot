using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Storage;
using VerifyBot.Services.Storage.Models;
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
            _publicKeyCert = new X509Certificate2(_verificationOptions.PublicKeyPath);
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
            RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();
            
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            
            // Encrypt username with RSA, used to allow username to be recovered.
            byte[] encryptedUsername;
            using (RSA rsa = _publicKeyCert.GetRSAPublicKey())
                encryptedUsername = rsa.Encrypt(usernameBytes, RSAEncryptionPadding.OaepSHA256);

            // Generate username hash, used for duplicate detection.
            byte[] salt = new byte[64]; // 512 bit random salt for hash
            rng.GetBytes(salt);
            
            // Combine username and salt for hashing.
            byte[] hashInput = new byte[usernameBytes.Length + salt.Length];
            usernameBytes.CopyTo(hashInput, 0);
            salt.CopyTo(hashInput, usernameBytes.Length);
            
            // Hash username with the salt
            byte[] hashedUsername;
            using (SHA512 sha = SHA512.Create())
                hashedUsername = sha.ComputeHash(hashInput);

            // Generate random Base32 token for user to verify with.
            byte[] tokenBuffer = new byte[RandomTokenLength];
            rng.GetBytes(tokenBuffer);
            string token = "$" + Base32.ToString(tokenBuffer);
            
            await _storageService.AddPendingVerificationAsync(new PendingVerification()
            {
                UserId = userId,
                Token = token,
                EncryptedUsername = encryptedUsername,
                UsernameSalt = salt,
                UsernameHash = hashedUsername
            });
            return token;
        }
        
        /// <summary>
        /// Checks if a uni username is valid
        /// </summary>
        public bool IsEmailValid(string email, out string username)
        {
            Match match = new Regex(_verificationOptions.EmailPattern).Match(email);
            if (!match.Success)
            {
                username = string.Empty;
                return false;
            }

            username = match.Groups[_verificationOptions.EmailUsernameMatchGroup].Value;
            return true;
        }
    }
}