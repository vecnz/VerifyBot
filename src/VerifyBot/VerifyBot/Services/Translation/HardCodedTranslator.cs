using System.Collections.Generic;
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
            { "VERIFY_COMMAND_EMAIL_TOKEN_OPTION_NAME", "email" },
            { "VERIFY_COMMAND_EMAIL_TOKEN_OPTION_DESCRIPTION", "Your VUW email address e.g. username@myvuw.ac.nz" },
            { "VERIFY_COMMAND_START_FAIL", ":fire:\tThere was an error while attempting to verify you."},
            { "VERIFY_COMMAND_START_INVALID_EMAIL", ":warning:\tThe email address you supplied is invalid or not allowed for verification."},
            { "VERIFY_COMMAND_START_ALREADY_VERIFIED", ":warning:\tYou are already verified."},
            { "VERIFY_COMMAND_START_SUCCESS", "::white_check_mark:\tSuccess! An email with further instructions should arrive in your inbox shortly."},
            { "VERIFY_COMMAND_FINISH_FAIL", ":fire:\tThere was an error while attempting to verify you."},
            { "VERIFY_COMMAND_FINISH_INVALID_TOKEN", ":warning:\tThe verification token you supplied is invalid."},
            { "VERIFY_COMMAND_FINISH_EXPIRED_TOKEN", ":warning:\tThe verification token you supplied is expired."},
            { "VERIFY_COMMAND_FINISH_SUCCESS", ":white_check_mark:\tSuccess! You are now verified."},
            
        };

        private readonly IReadOnlyDictionary<string, string> _translationMap;

        public HardCodedTranslator()
        {
            _translationMap = HardCodedTranslationMap;
        }

        public string T(string key, params object[] values)
        {
            return Translate(key, values);
        }

        public string Translate(string key, params object[] values)
        {
            if (!_translationMap.ContainsKey(key))
                throw new TranslationNotFoundException(key);
            return string.Format(_translationMap[key], values);
        }
    }
}