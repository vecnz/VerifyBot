using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VerifyBot.Services.Translation.Exceptions;

namespace VerifyBot.Services.Translation
{
    public class HardCodedTranslator : ITranslator
    {
        private static readonly IReadOnlyDictionary<string, string> HardCodedTranslationMap = new Dictionary<string, string>
        {
            { "SERVER_ERROR", ":fire:\tThe server encountered an error." },
            
            // Verify command
            { "VERIFY_COMMAND_NAME", "verify" },
            { "VERIFY_COMMAND_DESCRIPTION", "Verify your Discord account using your VUW email address." },
            { "VERIFY_COMMAND_EMAIL_OPTION_NAME", "email" },
            { "VERIFY_COMMAND_EMAIL_OPTION_DESCRIPTION", "Your VUW email address e.g. username@myvuw.ac.nz" },
            { "VERIFY_COMMAND_FAIL", ":fire:\tThere was an error while attempting to verify you."},
            { "VERIFY_COMMAND_INVALID_EMAIL", ":warning:\tThe email address you supplied is invalid or not allowed for verification."},
            { "VERIFY_COMMAND_ALREADY_VERIFIED", ":warning:\tYou are already verified."},
            { "VERIFY_COMMAND_SUCCESS", ":white_check_mark:\tSuccess! An email with further instructions should arrive in your inbox shortly."},

            // VerifyCode command
            { "VERIFY_CODE_COMMAND_NAME", "verifycode" },
            { "VERIFY_CODE_COMMAND_DESCRIPTION", "Complete account verification using a verification code." },
            { "VERIFY_CODE_COMMAND_TOKEN_OPTION_NAME", "code" },
            { "VERIFY_CODE_COMMAND_TOKEN_OPTION_DESCRIPTION", "The code that was emailed to you when you used the /verify command." },
            { "VERIFY_CODE_COMMAND_FAIL", ":fire:\tThere was an error while attempting to complete your verification."},
            { "VERIFY_CODE_COMMAND_INVALID_TOKEN", ":warning:\tThe verification token you supplied is invalid."},
            { "VERIFY_CODE_COMMAND_EXPIRED_TOKEN", ":warning:\tThe verification token you supplied is expired."},
            { "VERIFY_CODE_COMMAND_ALREADY_VERIFIED", ":warning:\tYou are already verified."},
            { "VERIFY_CODE_COMMAND_SUCCESS", ":white_check_mark:\tSuccess! You are now verified."},
        };

        private readonly IReadOnlyDictionary<string, string> _translationMap;
        private readonly ILogger<HardCodedTranslator> _logger;

        public HardCodedTranslator(ILogger<HardCodedTranslator> logger)
        {
            _translationMap = HardCodedTranslationMap;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string T(string key, params object[] values)
        {
            return Translate(key, values);
        }

        public string Translate(string key, params object[] values)
        {
            if (!_translationMap.ContainsKey(key))
            {
                _logger.LogError("Failed to find translation {key}", key);
                throw new TranslationNotFoundException(key);
            }

            return string.Format(_translationMap[key], values);
        }
    }
}