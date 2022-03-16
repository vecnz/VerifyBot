using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VerifyBot.Services.Email;
using VerifyBot.Services.Storage.MySql;
using VerifyBot.Services.Storage.MySql.TableModels;
using VerifyBot.Services.Verification.Configuration;
using VerifyBot.Services.Verification.Helpers;

namespace VerifyBot.Services.Verification
{
    public class VerificationService
    {
        private const int RandomTokenLength = 5; // Length in bytes. Base32 encodes 5 bytes into 8 characters.

        private readonly VerificationOptions _verificationOptions;
        private readonly MySqlStorageService _storageService;
        private readonly IEmailService _emailService;
        private readonly ILogger<VerificationService> _logger;
        private static readonly Regex TokenPattern = new Regex("^\\$[A-Z0-9]+$");

        public delegate void VerificationChangedEventHandler(VerificationService sender, ulong userId, bool verified);
        public static event VerificationChangedEventHandler VerificationChanged;
        
        public enum StartVerificationResult
        {
            Success,
            AlreadyVerified,
            InvalidEmail,
            Failure
        }

        public enum FinishVerificationResult
        {
            Success,
            InvalidToken,
            TokenExpired,
            Failure
        }

        public VerificationService(
            IOptions<VerificationOptions> verificationOptions,
            MySqlStorageService storageService,
            IEmailService emailService,
            ILogger<VerificationService> logger)
        {
            _verificationOptions = verificationOptions?.Value ?? throw new ArgumentNullException(nameof(verificationOptions));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<StartVerificationResult> StartVerificationAsync(ulong userId, string email)
        {
            try
            {
                _logger.LogInformation($"Start verification called by user ID {userId}", userId);
                if (!IsEmailValid(email, out string username))
                {
                    _logger.LogDebug($"Invalid verification email supplied by user ID {userId}", userId);
                    return StartVerificationResult.InvalidEmail;
                }

                _logger.LogTrace($"Creating verification token for user ID {userId}", userId);
                string token = await CreateVerificationTokenAsync(userId, username);
                
                _logger.LogTrace($"Sending verification email for user ID {userId}", userId);
                await _emailService.SendVerificationEmailAsync(email, token);
                
                _logger.LogDebug($"Start verification succeeded for user ID {userId}", userId);
                return StartVerificationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start verification. User ID {user}. Message {message}", userId, ex.Message);
                return StartVerificationResult.Failure;
            }
        }

        public async Task<FinishVerificationResult> FinishVerificationAsync(ulong userId, string token)
        {
            try
            {
                _logger.LogInformation($"Finish verification called by user ID {userId}", userId);
                PendingVerification pendingVerification = await _storageService.GetPendingVerificationAsync(userId, token);
                if (pendingVerification == null)
                {
                    _logger.LogDebug($"Invalid verification token supplied by user ID {userId}", userId);
                    return FinishVerificationResult.InvalidToken;
                }

                if (DateTimeOffset.FromUnixTimeSeconds(pendingVerification.creation_time) <
                    DateTimeOffset.UtcNow - _verificationOptions.VerificationTokenExpiry)
                {
                    _logger.LogDebug($"Expired verification token supplied by user ID {userId}", userId);
                    return FinishVerificationResult.TokenExpired;
                }

                _logger.LogTrace($"Setting verified username for user ID {userId}", userId);
                await _storageService.SetUserVerifiedUsernameId(userId, pendingVerification.username_record_id);
                
                _logger.LogTrace($"Calling verification changed event handler user ID {userId}", userId);
                VerificationChanged?.Invoke(this, userId, true);
                
                _logger.LogDebug($"Finish verification succeeded for user ID {userId}", userId);
                return FinishVerificationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to finish verification. User ID {user}. Message {message}", userId, ex.Message);
                return FinishVerificationResult.Failure;
            }
        }

        /// <summary>
        /// Checks if the specified string is the correct format to be a verification token.
        /// </summary>
        /// <param name="token">The token string to check.</param>
        /// <returns>True if the string is in the token format.</returns>
        public static bool IsVerificationToken(string token)
        {
            return TokenPattern.IsMatch(token.ToUpper());
        }
        
        private async Task<string> CreateVerificationTokenAsync(ulong userId, string username)
        {
            _logger.LogTrace("Creating new verification token for user ID {userId}", userId);
            // Generate random Base32 token for user to verify with.
            byte[] tokenBuffer = new byte[RandomTokenLength];
            new RNGCryptoServiceProvider().GetBytes(tokenBuffer);
            string token = "$" + Base32.ToString(tokenBuffer);

            await _storageService.AddPendingVerificationAsync(userId, token, username);
            _logger.LogTrace("Successfuly created new verification token for user ID {userId}", userId);
            return token;
        }

        /// <summary>
        /// Checks if a uni username is valid.
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
        
        public async Task<bool> IsUserVerifiedAsync(ulong userId)
        {
            _logger.LogTrace("Getting user verification status for user ID {userId}", userId);
            var user = await _storageService.GetUserAsync(userId);
            if (user == null)
            {
                _logger.LogTrace("User ID {userId} is {verified}", userId, "Unverified");
                return false;
            }
            
            
            bool verified = user.username_record_id != null;
            _logger.LogTrace("User ID {userId} is {verified}.", userId, verified ? "Verified" : "Unverified");
            return verified;
        }
    }
}