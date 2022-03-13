using System;
using Discord;
using VerifyBot.Services.Verification;

namespace VerifyBot.Services.DiscordBot.Commands
{
    public class VerifyCommand : ICommand
    {
        private readonly VerificationService _verificationService;
        
        public VerifyCommand(VerificationService verificationService)
        {
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
        }
        
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