using Discord;

namespace VerifyBot.Services.DiscordBot.Commands
{
    public interface ICommand
    {
        SlashCommandProperties Build();
        void Execute(ISlashCommandInteraction command);
    }
}