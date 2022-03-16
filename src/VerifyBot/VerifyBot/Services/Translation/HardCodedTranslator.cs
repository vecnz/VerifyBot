using System.Collections.Generic;
using VerifyBot.Services.Translation.Exceptions;

namespace VerifyBot.Services.Translation
{
    public class HardCodedTranslator : ITranslator
    {
        private static readonly IReadOnlyDictionary<string, string> HardCodedTranslationMap = new Dictionary<string, string>
        {
            { "SERVER_ERROR", ":fire:\tThe server encountered an error." },
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