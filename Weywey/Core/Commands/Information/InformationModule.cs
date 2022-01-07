using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Information;

public partial class InformationModule : ModuleBase<SocketCommandContext>
{
    [Name("Information")]
    [Command("info", RunMode = RunMode.Async)]
    [Alias("stats")]
    [Summary("Shows the bot's information.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task BotInfoCommand()
        => await InformationCommands.SendInfoAsync(Context.Channel as ITextChannel, Context.User);

    [Name("Guild Information")]
    [Command("guildinfo", RunMode = RunMode.Async)]
    [Summary("Shows the guild's information.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task GuildInfoCommand()
        => await InformationCommands.SendGuildInfoAsync(Context.Channel as ITextChannel, Context.User, Context.Guild);

    [Name("User Information")]
    [Command("userinfo", RunMode = RunMode.Async)]
    [Summary("Shows a user's information.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task UserInfoCommand([Remainder] SocketGuildUser user = null)
        => await InformationCommands.SendUserInfoAsync(Context.Channel as ITextChannel, Context.User, user);

    [Name("Role Information")]
    [Command("roleinfo", RunMode = RunMode.Async)]
    [Summary("Shows a guild role's information.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task RoleInfoCommand([Remainder] SocketRole role)
        => await InformationCommands.SendRoleInfoAsync(Context.Channel as ITextChannel, Context.User, role);

    [Name("Emote Information")]
    [Command("emoteinfo", RunMode = RunMode.Async)]
    [Summary("Shows a guild emote's information.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task EmoteInfoCommand([Remainder] GuildEmote emote)
        => await InformationCommands.SendEmoteInfoAsync(Context.Channel as ITextChannel, Context.User, emote);
}
