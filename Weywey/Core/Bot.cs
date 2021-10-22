using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Weywey.Core.Constants;
using Weywey.Core.Services;
using Weywey.Core.TypeReaders;

namespace Weywey.Core
{
    public class Bot
    {
        private DiscordSocketClient _client { get; set; }
        private CommandService _commands { get; set; }

        public Bot()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                GatewayIntents =
                    GatewayIntents.DirectMessages |
                    GatewayIntents.DirectMessageTyping |
                    GatewayIntents.DirectMessageReactions |
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildBans |
                    GatewayIntents.GuildEmojis |
                    GatewayIntents.GuildInvites |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.GuildWebhooks |
                    GatewayIntents.GuildPresences |
                    GatewayIntents.GuildVoiceStates |
                    GatewayIntents.GuildIntegrations |
                    GatewayIntents.GuildMessageTyping |
                    GatewayIntents.GuildMessageReactions,
                MessageCacheSize = 1000,
                AlwaysDownloadUsers = true
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = true,
                IgnoreExtraArgs = true
            });
            _commands.AddTypeReader<GuildEmote>(new EmoteTypeReader());
            _commands.AddTypeReader<Color>(new ColorTypeReader());

            var collection = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(new Random())
                .AddSingleton(new HttpClient());

            ProviderService.SetProvider(collection);
            _commands.Log += Log;
            _client.Log += Log;
            _client.Ready += OnReady;
            _client.MessageReceived += OnMessage;
        }

        public async Task RunAsync()
        {
            ConfigurationService.RunService();
            DataService.RunService();
            ReactionService.RunService();

            if (string.IsNullOrWhiteSpace(ConfigurationService.Token))
                return;
            
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), ProviderService.Provider);
            await _client.LoginAsync(TokenType.Bot, ConfigurationService.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage arg)
        {
            var log = $"[{DateTime.UtcNow.ToLongTimeString()}] ({arg.Severity.ToString().ToUpper()}) {arg.Source} => {((arg.Exception == null) ? arg.Message : arg.Exception.Message)}";
            
            if (arg.Exception?.InnerException != null)
                log += $"\n(INNER) => {arg.Exception.InnerException?.Message}";

            if (arg.Exception?.InnerException?.InnerException != null)
                log += $"\n(INNER) => {arg.Exception.InnerException?.InnerException?.Message}";
            
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            await _client.SetStatusAsync(UserStatus.Idle);
            await _client.SetGameAsync($"@{_client.CurrentUser.Username} help • Version {ConfigurationService.Version}", null, ActivityType.Watching);
            ReactionService.CompletePolls().GetAwaiter();
        }

        private async Task OnMessage(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            
            if (message.Author.IsBot || message.Channel is IDMChannel)
                return;
            
            var argPos = 0;

            if (!(message.HasStringPrefix(ConfigurationService.Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;

            var result = await _commands.ExecuteAsync(context, argPos, ProviderService.Provider);

            if (!result.IsSuccess)
            {
                if (result.Error is CommandError.UnknownCommand)
                    return;
                
                await context.Channel.SendMessageAsync($"❗ {GetErrorMessage(result)}");
            }
        }

        private static string GetErrorMessage(IResult result)
        {
            return result.Error switch
            {
                CommandError.ParseFailed => Messages.ParseFailed,
                CommandError.BadArgCount => Messages.BadArgumentCount,
                CommandError.ObjectNotFound => Messages.ObjectNotFound,
                CommandError.MultipleMatches => Messages.MultipleMatches,
                CommandError.UnmetPrecondition => Messages.UnmetPreconditions,
                CommandError.Exception => Messages.Exception,
                CommandError.Unsuccessful => Messages.Unsuccessful,
                _ => $"ERROR: {result}"
            };
        }
    }
}