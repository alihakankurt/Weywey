using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Weywey.Core.Modules.Moderation
{
    public partial class ModerationModule : ModuleBase<SocketCommandContext>
    {
        [Name("Kick")]
        [Command("kick", RunMode = RunMode.Async)]
        [Summary("Kicks guild members.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickCommand([Summary("The user that will be banned.")] SocketGuildUser target, [Remainder] [Summary("The reason of the kick.")] string reason = null)
        {
            await target.KickAsync($"{reason}  -Actioned by {Context.User}");
            await ReplyAsync($"**{target}** has kicked from server by {Context.User.Mention}.");
        }
    }
}