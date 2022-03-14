namespace VerifyBot.Services.Storage.MySql.Configuration
{
    public class MySqlStorageOptions
    {
        public const string Name = "MySql";

        public string ConnectionString { get; set; } = string.Empty;
        public string UsernameHashPepperB64 { get; set; } = string.Empty;
        public string UsernamePublicKeyPath { get; set; } = string.Empty;
    }
}