using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Moderation;

public partial class ModerationModule : ModuleBase<SocketCommandContext>
{
    [Name("Kick")]
    [Command("kick", RunMode = RunMode.Async)]
    [Summary("Kicks guild members.")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task KickCommand(SocketGuildUser target, [Remainder] string reason = null)
        => await ModerationCommands.KickAsync(Context.Channel as ITextChannel, Context.User, target, reason);

    [Name("Ban")]
    [Command("ban", RunMode = RunMode.Async)]
    [Summary("Bans guild members.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task BanCommand(SocketGuildUser target, [Remainder] string reason = null)
        => await ModerationCommands.BanAsync(Context.Channel as ITextChannel, Context.Guild, Context.User, target, reason);

    [Name("Unban")]
    [Command("unban", RunMode = RunMode.Async)]
    [Summary("Unbans banned Discord users.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task UnbanCommand(ulong targetId, [Remainder] string reason = null)
        => await ModerationCommands.UnbanAsync(Context.Channel as ITextChannel, Context.Guild, Context.User, targetId, reason);

    [Name("Clear")]
    [Command("clear", RunMode = RunMode.Async)]
    [Summary("Deletes spesific amount of messages.")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task ClearCommand(uint amount)
        => await ModerationCommands.ClearAsync(Context.Channel as ITextChannel, amount);

    [Name("Set Name")]
    [Command("setname", RunMode = RunMode.Async)]
    [Summary("Sets a user nickname in guild.")]
    [RequireUserPermission(GuildPermission.ManageNicknames)]
    public async Task SetNicknameCommand(SocketGuildUser user, [Remainder] string nickname = null)
        => await ModerationCommands.SetNameAsync(Context.Channel as ITextChannel, user, nickname);

    [Name("Reaction Role")]
    [Command("reaction-role", RunMode = RunMode.Async)]
    [Summary("Creates reaction role.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task ReactionRoleCommand(string roleIds, params GuildEmote[] emotes)
        => await ModerationCommands.CreateReactionRoleAsync(Context.Channel as ITextChannel, Context.Guild, Context.Message, roleIds.Split(';'), emotes);

    [Name("Giveaway")]
    [Command("giveaway", RunMode = RunMode.Async)]
    [Summary("Starts a new giveaway.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task GiveawayCommand(ITextChannel channel, [Remainder] string prize)
        => await ModerationCommands.StartGiveawayAsync(channel, Context.Message, Context.User, prize);
}
