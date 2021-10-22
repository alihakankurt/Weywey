using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Fun
{
    public partial class InformationModule : ModuleBase<SocketCommandContext>
    {
        [Name("Bot Information")]
        [Command("botinfo", RunMode = RunMode.Async)]
        [Alias("stats")]
        [Summary("Shows the bot's information.")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task BotInfoCommand()
        {
            var process = Process.GetCurrentProcess();

            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author.Name = Context.Client.CurrentUser.ToString();
                    author.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                })
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .AddField("Latency", $"{Context.Client.Latency} ms", true)
                .AddField(".NET Version", "Core 3.1", true)
                .AddField("C# Version", "9.0", true)
                .AddField("Bot Version", ConfigurationService.Version, true)
                .AddField("Discord.NET Version", "2.4.0", true)
                .AddField("RAM Usage", $"{process.PrivateMemorySize64 / 1048576} MB", true)
                .AddField("CPU Time", $"{process.TotalProcessorTime.TotalMilliseconds} ms", true)
                .WithColor((Context.Client.Latency < 100) ? Color.DarkGreen : ((Context.Client.Latency < 200) ? Color.Gold : Color.Red))
                .WithCurrentTimestamp().Build();

            await ReplyAsync(embed: embed);
        }
    }
}
