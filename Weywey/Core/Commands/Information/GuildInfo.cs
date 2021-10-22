using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Fun
{
    public partial class InformationModule : ModuleBase<SocketCommandContext>
    {
        [Name("Guild Information")]
        [Command("guildinfo", RunMode = RunMode.Async)]
        [Summary("Shows the guild's information.")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task GuildInfoCommand()
        {
            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author.Name = Context.Guild.ToString();
                    author.IconUrl = Context.Guild.IconUrl;
                })
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .AddField("Id", Context.Guild.Id, false)
                .AddField("Owner", Context.Guild.Owner.Mention, false)
                .AddField("Created At", Context.Guild.CreatedAt, false)
                .AddField("Channels", $"📁 {Context.Guild.Channels.Where(x => x.GetType() == typeof(SocketCategoryChannel)).Count()}\n💬 {Context.Guild.Channels.Where(x => x.GetType() == typeof(SocketTextChannel)).Count()}\n🔊 {Context.Guild.Channels.Where(x => x.GetType() == typeof(SocketVoiceChannel)).Count()}", false)
                .AddField("Members", $"🟢 {Context.Guild.Users.Where(x => x.Status == UserStatus.Online).Count()}\n🟡 {Context.Guild.Users.Where(x => x.Status == UserStatus.Idle).Count()}\n🔴 {Context.Guild.Users.Where(x => x.Status == UserStatus.DoNotDisturb || x.Status == UserStatus.AFK).Count()}\n⚫ {Context.Guild.Users.Where(x => x.Status == UserStatus.Offline || x.Status == UserStatus.Invisible).Count()}", false)
                .AddField("Emotes", Context.Guild.Emotes.Count, false)
                .AddField("System Channel", Context.Guild.SystemChannel == null ? "No system channel" : Context.Guild.SystemChannel.Mention)
                .AddField("Rules Channel", Context.Guild.RulesChannel == null ? "No rules channel" : Context.Guild.RulesChannel.Mention)
                .AddField("AFK Channel", Context.Guild.AFKChannel == null ? "No afk channel" : $"<#{Context.Guild.AFKChannel.Id}>", false)
                .AddField("AFK Timeout", $"{Context.Guild.AFKTimeout} seconds", false)
                .AddField("Premium Tier", (int)Context.Guild.PremiumTier, false)
                .WithDescription(Context.Guild.Description)
                .WithColor(Context.Guild.Owner.Roles.OrderByDescending(x => x.Position).First().Color)
                .WithThumbnailUrl(Context.Guild.BannerUrl)
                .WithCurrentTimestamp().Build();

            await ReplyAsync(embed: embed);
        }
    }
}
