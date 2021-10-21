using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Weywey.Core.Commands.Owner
{
    public partial class OwnerModule : ModuleBase<SocketCommandContext>
    {
        [Name("Exit")]
        [Command("exit")]
        [Summary("Closes the bot.")]
        [RequireOwner]
        public async Task ShutdownCommand([Summary("Exit code (0 for close, 1 for restart).")] short code)
        {
            switch (code)
            {
                case 0:
                    await ReplyAsync("Exiting...");
                    Environment.Exit(0);
                    break;

                case 1:
                    await ReplyAsync("Restarting...");
                    System.Diagnostics.Process.Start(Environment.OSVersion.Platform == PlatformID.Win32NT ? "Weywey.exe" : "dotnet run Weywey.dll");
                    Environment.Exit(0);
                    break;

                default:
                    break;
            }
        }
    }
}
