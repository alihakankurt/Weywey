using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Owner;

public partial class OwnerModule : ModuleBase<SocketCommandContext>
{
    [Name("Exit")]
    [Command("exit")]
    [Summary("Closes the bot.")]
    [RequireOwner]
    public async Task ShutdownCommand([Remainder][Summary("Exit code (0 for close, 1 for restart)")] short code)
    {
        switch (code)
        {
            case 0:
                await ReplyAsync("Exiting...");
                Environment.Exit(0);
                break;

            case 1:
                await ReplyAsync("Restarting...");
                System.Diagnostics.Process.Start("dotnet Weywey.dll");
                Environment.Exit(0);
                break;

            default:
                break;
        }
    }
}
