namespace VerifyBot.Services.Storage.Configuration
{
    public class StorageOptions
    {
        public const string Name = "Storage";

        public string MySqlConnectionString { get; set; } = string.Empty;
    }
}