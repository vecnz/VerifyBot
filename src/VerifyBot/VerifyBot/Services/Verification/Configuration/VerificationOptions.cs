namespace VerifyBot.Services.Verification.Configuration
{
    public class VerificationOptions
    {
        public const string Name = "Verification";

        public string EmailPattern { get; set; } = string.Empty;
        public int EmailUsernameMatchGroup { get; set; } = 0;
        public string PublicKeyPath { get; set; } = string.Empty;
    }
}