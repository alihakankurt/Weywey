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
        private DiscordSocketClient Client { get; set; }
        private CommandService Commands { get; set; }

        public Bot()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
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

            Commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = true,
                IgnoreExtraArgs = true
            });
            Commands.AddTypeReader<GuildEmote>(new EmoteTypeReader());
            Commands.AddTypeReader<Color>(new ColorTypeReader());

            var collection = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(Commands)
                .AddSingleton(new Random())
                .AddSingleton(new HttpClient());

            ProviderService.SetProvider(collection);
            Commands.Log += Log;
            Client.Log += Log;
            Client.Ready += OnReady;
            Client.MessageReceived += OnMessage;
        }

        public async Task RunAsync()
        {
            ConfigurationService.RunService();
            
            if (string.IsNullOrWhiteSpace(ConfigurationService.Token))
                return;

            DataService.RunService();
            InteractionService.RunService();
            ReactionsRoleService.RunService();
            PollService.RunService();
            GiveawayService.RunService();

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), ProviderService.Provider);
            await Client.LoginAsync(TokenType.Bot, ConfigurationService.Token);
            await Client.StartAsync();

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
            await Client.SetStatusAsync(UserStatus.Idle);
            await Client.SetGameAsync($"@{Client.CurrentUser.Username} help • Version {ConfigurationService.Version}", null, ActivityType.Watching);
        }

        private async Task OnMessage(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(Client, message);
            
            if (message.Author.IsBot || message.Channel is IDMChannel)
                return;
            
            var argPos = 0;

            if (!(message.HasStringPrefix(ConfigurationService.Prefix, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos)))
                return;

            var result = await Commands.ExecuteAsync(context, argPos, ProviderService.Provider);

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
                _ => $"ERROR: {result.Error}"
            };
        }
    }
}