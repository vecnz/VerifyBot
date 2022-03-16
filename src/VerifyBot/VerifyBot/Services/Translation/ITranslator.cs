namespace VerifyBot.Services.Translation
{
    public interface ITranslator
    {
        public string Translate(string key, params object[] values);
        public string T(string key, params object[] values);
    }
}