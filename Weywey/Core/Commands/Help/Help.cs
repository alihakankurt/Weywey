using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weywey.Core.Extensions;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Help
{
    public partial class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Name("Help")]
        [Command("help", RunMode = RunMode.Async)]
        [Summary("Shows the help page.")]
        public async Task HelpCommand()
        {
            var _commands = ProviderService.GetService<CommandService>();
            var commands = await _commands.GetExecutableCommandsAsync(Context, ProviderService.Provider);
            var pages = commands.OrderBy(x => x.Module.Name).Select((x, i) =>
                new EmbedBuilder()
                    .WithFooter(footer =>
                    {
                        footer.Text = $"Requested by {Context.User} | Page {i + 1}/{commands.Count}";
                        footer.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithTitle(x.Name)
                    .AddField("Summary", x.Summary, false)
                    .AddField("Syntax", x.GetSyntax(), false)
                    .AddField("Module", x.Module.Name.Replace("Module", ""), false)
                    .AddField("Permissions", x.Preconditions.Where(x => x is RequireUserPermissionAttribute).Count() > 0 ? string.Join("\n", x.Preconditions.Where(x => x is RequireUserPermissionAttribute).Select(x => ((x as RequireUserPermissionAttribute) is var p && p.GuildPermission.HasValue ? p.GuildPermission.ToString() : p.ChannelPermission.ToString()).SeperateFromCaps())) : "No permission required.", false)
                    .WithColor(Color.Blue).Build()
            );
            
            await ReactionService.PaginateAsync(Context.Channel, Context.User, pages);
        }
    }
}
