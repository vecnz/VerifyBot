using System.Threading.Tasks;
using Discord;

namespace VerifyBot.Services.DiscordBot.Commands
{
    public interface ICommand
    {
        string Name { get; }
        SlashCommandProperties Build();
        Task ExecuteAsync(ISlashCommandInteraction command);
    }
}