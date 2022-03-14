namespace VerifyBot.Services.Storage.Models
{
    public class PendingVerification
    {
        public ulong UserId { get; set; }
        public string Token { get; set; }
        public byte[] EncryptedUsername { get; set; }
        public byte[] UsernameSalt { get; set; }
        public byte[] UsernameHash { get; set; }
    }
}