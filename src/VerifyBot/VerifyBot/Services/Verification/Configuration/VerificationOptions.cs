namespace VerifyBot.Services.Verification.Configuration
{
    public class VerificationOptions
    {
        public const string Name = "Verification";

        public string EmailPattern { get; set; } = string.Empty;
        public string PublicKeyPath { get; set; } = string.Empty;
    }
}