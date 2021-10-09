using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Weywey.Core.Commands.Fun
{
    public partial class InformationModule : ModuleBase<SocketCommandContext>
    {
        [Name("Role Information")]
        [Command("roleinfo", RunMode = RunMode.Async)]
        [Summary("Shows a guild role's information.")]
        public async Task RoleInfoCommand([Remainder] [Summary("What information will be shown")] SocketRole role)
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
                .AddField("Id", role.Id, false)
                .AddField("Color", role.Color, false)
                .AddField("Created At", role.CreatedAt, false)
                .AddField("Members", role.Members.Count(), false)
                .AddField("Is hoist?", role.IsHoisted, false)
                .AddField("Is managed?", role.IsManaged, false)
                .AddField("Permission Value", role.Permissions.RawValue, false)
                .WithDescription($"{role.Mention}'s Information")
                .WithColor(role.Color)
                .WithCurrentTimestamp().Build();
            await ReplyAsync(embed: embed);
        }
    }
}
