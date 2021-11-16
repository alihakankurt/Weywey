using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Moderation;

public partial class ModerationModule : ModuleBase<SocketCommandContext>
{
    [Name("Ban")]
    [Command("ban", RunMode = RunMode.Async)]
    [Summary("Bans guild members.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task BanCommand([Summary("The user that will be banned")] SocketGuildUser target, [Remainder][Summary("The reason of the ban")] string reason = null)
    {
        await target.BanAsync(0, $"{reason}  -Actioned by {Context.User}");
        await ReplyAsync($"**{target}** has banned from server by {Context.User.Mention}.");
    }
}
