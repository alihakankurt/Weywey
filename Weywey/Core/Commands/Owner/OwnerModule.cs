using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Owner;

public partial class OwnerModule : ModuleBase<SocketCommandContext>
{
    [Name("Exit")]
    [Command("exit")]
    [Summary("Closes the bot.")]
    [RequireOwner]
    public async Task ExitCommand([Remainder] ushort code)
        => await OwnerCommands.ExitAsync(Context.Channel as ITextChannel, code);

    [Name("Send")]
    [Command("send")]
    [Summary("Sends message to target channel.")]
    [RequireOwner]
    public async Task SendCommand(ulong id, [Remainder] string content)
        => await OwnerCommands.SendAsync(id, content);

    [Name("Test")]
    [Command("test")]
    [Summary("Test Command.")]
    [RequireOwner]
    public async Task TestCommand()
        => await OwnerCommands.TestAsync(Context.Channel as ITextChannel);
}
