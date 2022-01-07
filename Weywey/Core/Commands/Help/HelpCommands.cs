using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Help;

public static class HelpCommands
{
    private static readonly DiscordSocketClient Client;
    private static readonly CommandService Commands;
    private static readonly Regex Capital;

    static HelpCommands()
    {
        Client = ProviderService.GetService<DiscordSocketClient>();
        Commands = ProviderService.GetService<CommandService>();
        Capital = new(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
    }

    public static async Task HelpAsync(ITextChannel channel, IUser user)
    {
        var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {user.Username}";
                    footer.IconUrl = user.GetAvatarUrl();
                })
                .WithTitle($"{Client.CurrentUser.Username}'s Help Page")
                .WithDescription($"Type `{ConfigurationService.Prefix}help <command>` to get more details about a command. \n\n {string.Join('\n', Commands.Modules.Select(module => $"> **{module.Name}**\n{(string.Join("\n", module.Commands.Select(c => $"> - `{c.Name}:` {c.Summary}")))}"))}")
                .WithColor(Color.Teal)
                .WithCurrentTimestamp()
                .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task CommandHelpAsync(ITextChannel channel, IUser user, string commandName)
    {
        var command = Commands.Commands.FirstOrDefault(c => c.Aliases.Contains(commandName));
        if (command == null)
            await channel.SendMessageAsync($"Couldn't found any command with `{commandName}`.");

        else
        {
            var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {user}";
                    footer.IconUrl = user.GetAvatarUrl();
                })
                .WithTitle($"{command.Name}")
                .AddField("Summary", command.Summary, false)
                .AddField("Syntax", $"```cs\n\"{ConfigurationService.Prefix}{string.Join(" | ", command.Aliases)} {string.Join(' ', command.Parameters.Select(x => x.IsOptional ? $"[{x.Name}]" : $"<{x.Name}>"))}\"```", false)
                .AddField("Module", command.Module.Name.Replace("Module", ""), false)
                .AddField("Permissions", command.Preconditions.Any(x => x is RequireUserPermissionAttribute) ? string.Join('\n', command.Preconditions.Select(x => Capital.Replace((x as RequireUserPermissionAttribute is var p && p.GuildPermission.HasValue) ? p.GuildPermission.ToString() : p.ChannelPermission.ToString(), ""))) : "No permission required.", false)
                .WithColor(Color.Teal)
                .Build();
            await channel.SendMessageAsync(embed: embed);
        }
    }
}
