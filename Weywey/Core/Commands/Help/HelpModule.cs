using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Help;

public partial class HelpModule : ModuleBase<SocketCommandContext>
{
    [Name("Help")]
    [Command("help", RunMode = RunMode.Async)]
    [Summary("Shows the help page.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task HelpCommand([Remainder] string command = null)
        => await ((command == null) ? HelpCommands.HelpAsync(Context.Channel as ITextChannel, Context.User) : HelpCommands.CommandHelpAsync(Context.Channel as ITextChannel, Context.User, command));
}
