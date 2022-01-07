using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Owner;

public static class OwnerCommands
{
    private static readonly DiscordSocketClient Client;

    static OwnerCommands()
    {
        Client = ProviderService.GetService<DiscordSocketClient>();
    }

    public static async Task ExitAsync(ITextChannel channel, ushort code)
    {
        switch (code)
        {
            case 0:
                await channel.SendMessageAsync("Exiting...");
                Environment.Exit(0);
                break;

            case 1:
                await channel.SendMessageAsync("Restarting...");
                System.Diagnostics.Process.Start("dotnet Weywey.dll");
                Environment.Exit(0);
                break;

            default:
                break;
        }
    }

    public static async Task SendAsync(ulong id, string content)
    {
        if (Client.GetChannel(id) is IMessageChannel channel)
            await channel.SendMessageAsync(content);

        else if (Client.GetUser(id) is IUser user && !user.IsBot)
            await user.SendMessageAsync(content);
    }

    public static async Task TestAsync(ITextChannel channel)
    {
        await channel.SendMessageAsync("Tested.");
    }
}
