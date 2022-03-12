namespace VerifyBot.Services.DiscordBot.Configuration
{
    public class DiscordBotOptions
    {
        public const string Name = "DiscordBot";

        public string Token { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
    }
}