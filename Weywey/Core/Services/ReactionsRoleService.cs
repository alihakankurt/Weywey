using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weywey.Core.Entities;

namespace Weywey.Core.Services;

public static class ReactionsRoleService
{
    private static readonly string Path = "ReactionRoles.json";
    private static List<ReactionRoleItem> ReactionRoles { get; set; }
    private static DiscordSocketClient Client { get; set; }

    public static void RunService()
    {
        ReactionRoles = DataService.Get<List<ReactionRoleItem>>(Path);
        Client = ProviderService.GetService<DiscordSocketClient>();

        Client.ReactionAdded += Client_ReactionAdded;
        Client.ReactionRemoved += Client_ReactionRemoved;
        Client.MessageDeleted += Client_MessageDeleted;
        Client.RoleDeleted += Client_RoleDeleted;
    }

    public static void Add(ulong messageId, Dictionary<string, ulong> roles)
    {
        ReactionRoles.Add(new ReactionRoleItem
        {
            MessageId = messageId,
            Roles = roles
        });
        DataService.Save(Path, ReactionRoles);
    }

    public static void Remove(Predicate<ReactionRoleItem> filter)
    {
        int count = ReactionRoles.RemoveAll(filter);
        if (count > 0)
            DataService.Save(Path, ReactionRoles);
    }

    private static async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg, ISocketMessageChannel channel, SocketReaction reaction)
    {
        if (reaction.User.Value.IsBot || (channel as SocketTextChannel) == null)
            return;

        var message = await arg.GetOrDownloadAsync();

        foreach (var item in ReactionRoles)
            if (item.MessageId == message.Id)
                if (item.Roles.ContainsKey(reaction.Emote.ToString()))
                    await (reaction.User.Value as SocketGuildUser).AddRoleAsync(item.Roles[reaction.Emote.ToString()]);
    }

    private static async Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg, ISocketMessageChannel channel, SocketReaction reaction)
    {
        if (reaction.User.Value.IsBot || (channel as SocketTextChannel) == null)
            return;

        var message = await arg.GetOrDownloadAsync();

        foreach (var item in ReactionRoles)
            if (item.MessageId == message.Id)
                if (item.Roles.ContainsKey(reaction.Emote.ToString()))
                    await (reaction.User.Value as SocketGuildUser).RemoveRoleAsync(item.Roles[reaction.Emote.ToString()]);
    }

    private static Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg, ISocketMessageChannel channel)
    {
        if ((channel as SocketTextChannel) == null)
            return Task.CompletedTask;

        Remove(x => x.MessageId == arg.Id);
        return Task.CompletedTask;
    }

    private static Task Client_RoleDeleted(SocketRole role)
    {
        foreach (var item in ReactionRoles)
            if (item.Roles.ContainsValue(role.Id))
                item.Roles.Remove(item.Roles.Single(x => x.Value == role.Id).Key);

        return Task.CompletedTask;
    }
}
