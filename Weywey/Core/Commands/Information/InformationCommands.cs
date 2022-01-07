using Discord;
using Discord.WebSocket;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Information;

public static class InformationCommands
{
    private static readonly DiscordSocketClient Client;

    static InformationCommands()
    {
        Client = ProviderService.GetService<DiscordSocketClient>();
    }

    public static async Task SendInfoAsync(ITextChannel channel, IUser user)
    {
        var process = Process.GetCurrentProcess();
        
        var embed = new EmbedBuilder()
            .WithAuthor(author =>
            {
                author.Name = Client.CurrentUser.ToString();
                author.IconUrl = Client.CurrentUser.GetAvatarUrl();
            })
            .WithFooter(footer =>
            {
                footer.Text = $"Requested by {user.Username}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .AddField("Latency", $"{Client.Latency} ms", true)
            .AddField(".NET Version", "Core 3.1", true)
            .AddField("C# Version", "10.0", true)
            .AddField("Bot Version", ConfigurationService.Version, true)
            .AddField("Discord.NET Version", "3.1.0", true)
            .AddField("RAM Usage", $"{process.PrivateMemorySize64 / 1048576} MB", true)
            .AddField("CPU Time", $"{process.TotalProcessorTime.TotalMilliseconds} ms", true)
            .WithColor((Client.Latency < 100) ? Color.DarkGreen : ((Client.Latency < 200) ? Color.Gold : Color.Red))
            .WithCurrentTimestamp()
            .Build();

        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task SendGuildInfoAsync(ITextChannel channel, IUser user, SocketGuild guild)
    {
        int categoryChannels = guild.Channels.Count(x => x.GetType() == typeof(SocketCategoryChannel));
        int textChannels = guild.Channels.Count(x => x.GetType() == typeof(SocketTextChannel));
        int voiceChannels = guild.Channels.Count(x => x.GetType() == typeof(SocketVoiceChannel));

        int online = guild.Users.Count(x => x.Status == UserStatus.Online);
        int idle = guild.Users.Count(x => x.Status == UserStatus.Idle);
        int doNotDisturb = guild.Users.Count(x => x.Status == UserStatus.DoNotDisturb);
        int offline = guild.Users.Count(x => x.Status == UserStatus.Offline);

        var embed = new EmbedBuilder()
            .WithAuthor(author =>
            {
                author.Name = guild.Name;
                author.IconUrl = guild.IconUrl;
            })
            .WithFooter(footer =>
            {
                footer.Text = $"Requested by {user.Username}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .AddField("Id", guild.Id, false)
            .AddField("Owner", guild.Owner.Mention, false)
            .AddField("Created At", guild.CreatedAt, false)
            .AddField("Channels", $"📁 {categoryChannels}\n💬 {textChannels}\n🔊 {voiceChannels}", false)
            .AddField("Members", $"🟢 {online}\n🟡 {idle}\n🔴 {doNotDisturb}\n⚫ {offline}", false)
            .AddField("Emotes", guild.Emotes.Count, false)
            .AddField("System Channel", guild.SystemChannel?.Mention, false)
            .AddField("Rules Channel", guild.RulesChannel?.Mention, false)
            .AddField("AFK Channel", guild.AFKChannel?.Mention, false)
            .AddField("AFK Timeout", $"{guild.AFKTimeout} seconds", false)
            .AddField("Premium Tier", (int)guild.PremiumTier, false)
            .WithDescription(guild.Description)
            .WithColor(guild.Owner.Roles.OrderByDescending(x => x.Position).First().Color)
            .WithThumbnailUrl(guild.BannerUrl)
            .WithCurrentTimestamp()
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task SendUserInfoAsync(ITextChannel channel, IUser user, SocketGuildUser guildUser)
    {
        guildUser ??= user as SocketGuildUser;
        var topRole = guildUser.Roles.OrderByDescending(x => x.Position).First();
        var embed = new EmbedBuilder()
            .WithAuthor(author =>
            {
                author.Name = guildUser.Username;
                author.IconUrl = guildUser.GetAvatarUrl();
            })
            .WithFooter(footer =>
            {
                footer.Text = $"Requested by {user.Username}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .AddField("Id", guildUser.Id, false)
            .AddField("Is bot?", guildUser.IsBot, false)
            .AddField("Status", guildUser.Status, false)
            .AddField("Activity", ((guildUser.Activities.Count == 0) ? "No activity" : string.Join("\n", guildUser.Activities.Select(x => $"`{x.Type} {x.Name}` {x.Details}"))), false)
            .AddField("Top Role", topRole.Mention)
            .AddField("Created At", guildUser.CreatedAt, false)
            .AddField("Joined At", guildUser.JoinedAt, false)
            .WithColor(topRole.Color)
            .WithCurrentTimestamp()
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task SendRoleInfoAsync(ITextChannel channel, IUser user, SocketRole role)
    {
        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = $"Requested by {user.Username}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .AddField("Id", role.Id, false)
            .AddField("Color", role.Color, false)
            .AddField("Created At", role.CreatedAt, false)
            .AddField("Members", role.Members.Count(), false)
            .AddField("Is hoist?", role.IsHoisted, false)
            .AddField("Is managed?", role.IsManaged, false)
            .AddField("Permission Value", role.Permissions.RawValue, false)
            .WithDescription($"{role.Mention}'s Information")
            .WithColor(role.Color)
            .WithCurrentTimestamp()
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task SendEmoteInfoAsync(ITextChannel channel, IUser user, GuildEmote emote)
    {
        var embed = new EmbedBuilder()
            .WithAuthor(author =>
            {
                author.Name = emote.Name;
                author.IconUrl = emote.Url;
            })
            .WithFooter(footer =>
            {
                footer.Text = $"Requested by {user.Username}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .AddField("Id", emote.Id, false)
            .AddField("Created At", emote.CreatedAt, false)
            .AddField("Creator Id", emote.CreatorId, false)
            .AddField("Is animated?", emote.Animated, false)
            .WithColor(Color.Teal)
            .WithCurrentTimestamp()
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }
}
