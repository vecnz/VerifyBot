using System;

namespace VerifyBot.Services.Translation.Exceptions
{
    public class TranslationNotFoundException : Exception
    {
        public readonly string TranslationKey;
        public TranslationNotFoundException(string key)
        {
            TranslationKey = key;
        }
    }
}