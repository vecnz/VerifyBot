using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VerifyBot.Services.DiscordBot.Configuration;
using VerifyBot.Services.Storage.MySql;
using VerifyBot.Services.Verification;

namespace VerifyBot.Services.DiscordBot
{
    public class DiscordBot : IHostedService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly DiscordBotOptions _botOptions;
        private readonly MySqlStorageService _storageService;
        private readonly VerificationService _verificationService;
        private readonly ILogger<DiscordBot> _logger;

        private readonly IReadOnlyDictionary<LogSeverity, LogLevel> _logLevelMap = // Maps Discord.NET logging levels to Microsoft extensions logging levels.
            new Dictionary<LogSeverity, LogLevel>
            {
                { LogSeverity.Debug, LogLevel.Trace },
                { LogSeverity.Verbose, LogLevel.Debug },
                { LogSeverity.Info, LogLevel.Information },
                { LogSeverity.Warning, LogLevel.Warning },
                { LogSeverity.Error, LogLevel.Error },
                { LogSeverity.Critical, LogLevel.Critical },
            };
        
        public DiscordBot(
            DiscordSocketClient discordClient,
            IOptions<DiscordBotOptions> botOptions,
            MySqlStorageService storageService,
            VerificationService verificationService,
            ILogger<DiscordBot> logger,
            SlashCommandHandler slashCommandHandler)
        {
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _botOptions = botOptions?.Value ?? throw new ArgumentNullException(nameof(botOptions));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _discordClient.Log += DiscordClientOnLog;
            _discordClient.JoinedGuild += DiscordClientOnJoinedGuild;
            _discordClient.GuildMemberUpdated += DiscordClientOnGuildMemberUpdated;
            _discordClient.Ready += DiscordClientOnReady;
            _discordClient.UserJoined += DiscordClientOnUserJoined;
            _discordClient.RoleDeleted += DiscordClientOnRoleDeleted;
            slashCommandHandler.Register();
            VerificationService.VerificationChanged += VerificationServiceOnVerificationChanged;
        }

        private async Task DiscordClientOnRoleDeleted(SocketRole arg)
        {
            await createVerifiedRoleAsync(arg.Guild);
        }

        private async Task DiscordClientOnGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser arg2)
        {
            await updateUserVerificationAsync(arg2, arg2.Guild);
        }

        private async Task DiscordClientOnUserJoined(SocketGuildUser arg)
        {
            await updateUserVerificationAsync(arg, arg.Guild);
        }

        private async Task DiscordClientOnReady()
        {
            foreach (var guild in _discordClient.Guilds)
            {
                await createVerifiedRoleAsync(guild);
            }
        }

        private async Task DiscordClientOnJoinedGuild(SocketGuild arg)
        {
            await createVerifiedRoleAsync(arg);
        }

        private async void VerificationServiceOnVerificationChanged(VerificationService sender, ulong userid, bool verified)
        {
            await updateUserVerificationAsync(_discordClient.GetUser(userid));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discord bot starting...");
            await _discordClient.SetActivityAsync(new Game(_botOptions.StatusText));
            
            _logger.LogDebug("Logging in...");
            await _discordClient.LoginAsync(TokenType.Bot, _botOptions.Token);
            _logger.LogDebug("Connecting to websocket...");
            await _discordClient.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordClient.StopAsync();
        }
        
        private Task DiscordClientOnLog(LogMessage arg)
        {
            LogLevel level = _logLevelMap[arg.Severity];
            _logger.Log(level, "{source} {message}", arg.Source, arg.Message, arg.Exception);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates a users verification role across all mutual guilds.
        /// </summary>
        private async Task updateUserVerificationAsync(SocketUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            bool isVerified = await _verificationService.IsUserVerifiedAsync(user.Id);

            foreach (SocketGuild guild in user.MutualGuilds)
            {
                await updateUserVerificationAsync(user, guild, isVerified);
            }
        }

        private async Task updateUserVerificationAsync(SocketUser user, SocketGuild guild)
        {
            bool isVerified = await _verificationService.IsUserVerifiedAsync(user.Id);
            await updateUserVerificationAsync(user, guild, isVerified);
        }
        
        private async Task updateUserVerificationAsync(SocketUser user, SocketGuild guild, bool isVerified)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                _logger.LogDebug("Checking user {uname} {uid} verification status in {gname} {gid}", user.Username, user.Id, guild.Name, guild.Id);
                
                ulong? verifiedRoleId = (await _storageService.GetGuildAsync(guild.Id))?.verified_role_id;
                if (verifiedRoleId == null)
                {
                    _logger.LogWarning("Guild {name} {id} is missing a verified role.", guild.Name, guild.Id);
                    return;
                }

                SocketRole verifiedRole = guild.GetRole(verifiedRoleId.Value);
                if (verifiedRole == null)
                {
                    _logger.LogWarning("Verified role has been deleted in guild {name} {id}", guild.Name, guild.Id);
                    return;
                }
                
                SocketGuildUser guildUser = guild.GetUser(user.Id);
                if (guildUser == null)
                {
                    _logger.LogError("User {uname} {uid} missing from guild {gname} {gid}", user.Username, user.Id, guild.Name, guild.Id);
                    return;
                }

                if (isVerified)
                {
                    if (!guildUser.Roles.Any(x => x.Id == verifiedRoleId))
                    {
                        _logger.LogInformation("Giving user {uname} {uid} verified role in guild {gname} {gid}", user.Username, user.Id, guild.Name, guild.Id);
                        await guildUser.AddRoleAsync(verifiedRole);
                    }
                }
                else
                {
                    if (guildUser.Roles.Any(x => x.Id == verifiedRoleId))
                    {
                        _logger.LogInformation("Removing verified role from user {uname} {uid} in guild {gname} {gid}", user.Username, user.Id, guild.Name, guild.Id);
                        await guildUser.RemoveRoleAsync(verifiedRole);
                    }
                }
                
                _logger.LogTrace("User {uname} {uid} has correct verification status in {gname} {gid}", user.Username, user.Id, guild.Name, guild.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user verification in guild {name} {id} {message}", guild.Name, guild.Id, ex.Message);
            }
        }

        /// <summary>
        /// Creates a verified role for a guild if it doesn't exist.
        /// </summary>
        private async Task createVerifiedRoleAsync(SocketGuild guild)
        {
            try
            {
                _logger.LogTrace("Checking if verified role exists in {gname} {gid}", guild.Name, guild.Id);
                
                ulong? verifiedRoleId = (await _storageService.GetGuildAsync(guild.Id))?.verified_role_id;
                if (verifiedRoleId == null)
                {
                    _logger.LogInformation("Adding new verified role in guild {name} {id}. No role found in guild settings.", guild.Name, guild.Id);
                    var role = await guild.CreateRoleAsync(_botOptions.DefaultVerifiedRoleName);
                    await _storageService.SetGuildVerifiedRoleId(guild.Id, role.Id);
                    return;
                }
                
                SocketRole verifiedRole = guild.GetRole(verifiedRoleId.Value);
                if (verifiedRole == null)
                {
                    _logger.LogWarning("Adding new verified role in guild {name} {id}. Role {rid} not found in Discord.", guild.Name, guild.Id, verifiedRoleId.Value);
                    var role = await guild.CreateRoleAsync(_botOptions.DefaultVerifiedRoleName);
                    await _storageService.SetGuildVerifiedRoleId(guild.Id, role.Id);
                }
                
                _logger.LogTrace("Verified role already exists in guild {gname} {gid}", guild.Name, guild.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create verified role in guild {name} {id} {message}", guild.Name, guild.Id, ex.Message);
            }
        }
    }
}