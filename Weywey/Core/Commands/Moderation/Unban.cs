using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Weywey.Core.Modules.Moderation;

public partial class ModerationModule : ModuleBase<SocketCommandContext>
{
    [Name("Unban")]
    [Command("unban", RunMode = RunMode.Async)]
    [Summary("Unbans banned Discord users.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task UnbanCommand([Summary("The id of target to unban")] ulong targetId, [Remainder][Summary("The reason of the unban")] string reason = null)
    {
        var ban = await Context.Guild.GetBanAsync(targetId);

        if (ban == null)
        {
            await ReplyAsync("Target is not banned!");
        }

        else
        {
            await Context.Guild.RemoveBanAsync(ban.User.Id, new RequestOptions() { AuditLogReason = $"{reason}  -Actioned by {Context.User}" });
            await ReplyAsync($"**{ban.User}**'s ban removed in server by {Context.User.Mention}.");
        }
    }
}
