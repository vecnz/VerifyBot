using Discord;

namespace VerifyBot.Services.DiscordBot.Commands
{
    public class VerifyCommand : ICommand
    {
        public SlashCommandProperties Build()
        {
            return new SlashCommandBuilder()
                .WithName("verify")
                .WithDescription("TODO: put this into translations.")
                .AddOption("email", ApplicationCommandOptionType.String, "TODO: Translate this also", true)
                .Build();
        }

        public async void Execute(ISlashCommandInteraction command)
        {
            
        }
    }
}