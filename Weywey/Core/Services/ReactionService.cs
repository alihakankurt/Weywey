using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weywey.Core.Constants;
using Weywey.Core.Entities;

namespace Weywey.Core.Services;

public static class InteractionService
{
    private static DiscordSocketClient Client { get; set; }
    private static List<WaitlistReactionItem> WaitlistReactions { get; set; }
    private static List<WaitlistMessageItem> WaitlistMessages { get; set; }

    public static void RunService()
    {
        Client = ProviderService.GetService<DiscordSocketClient>();
        WaitlistReactions = new List<WaitlistReactionItem>();
        WaitlistMessages = new List<WaitlistMessageItem>();

        Client.ReactionAdded += Client_ReactionAdded;
        Client.MessageReceived += Client_MessageReceived;
    }

    private static async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg, ISocketMessageChannel channel, SocketReaction reaction)
    {
        if (reaction.User.Value.IsBot || (channel as SocketTextChannel) == null)
            return;

        var message = await arg.GetOrDownloadAsync();

        foreach (var item in WaitlistReactions.Where(x => !x.IsExpired()))
            if (item.MessageId == message.Id)
                if (item.Filter == null)
                    item.Reaction = reaction;

                else if (item.Filter(reaction))
                    item.Reaction = reaction;
    }

    private static Task Client_MessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot || (message.Channel as SocketTextChannel) == null)
            return Task.CompletedTask;

        foreach (var item in WaitlistMessages.Where(x => !x.IsExpired()))
            if (item.ChannelId == message.Channel.Id)
                if (item.Filter == null)
                    item.Message = message;

                else if (item.Filter(message))
                    item.Message = message;

        return Task.CompletedTask;
    }

    public static async Task<SocketReaction> WaitForReactionAsync(ulong messageId, TimeSpan duration, Predicate<SocketReaction> filter = null)
    {
        var item = new WaitlistReactionItem(messageId, duration, filter);

        WaitlistReactions.Add(item);

        while (item.Reaction == null && !item.IsExpired())
        {
            await Task.Delay(1);
        }

        WaitlistReactions.Remove(item);

        return item.Reaction;
    }

    public static async Task<SocketMessage> WaitForMessageAsync(ulong channelId, TimeSpan duration, Predicate<SocketMessage> filter = null)
    {
        var item = new WaitlistMessageItem(channelId, duration, filter);

        WaitlistMessages.Add(item);

        while (item.Message == null && !item.IsExpired())
        {
            await Task.Delay(1);
        }

        WaitlistMessages.Remove(item);

        return item.Message;
    }

    public static async Task PaginateAsync(IMessageChannel channel, IUser user, IEnumerable<Embed> embeds)
    {
        if (embeds.Count() < 1)
            return;

        int index = 0;
        DateTime start = DateTime.UtcNow;
        var message = await channel.SendMessageAsync(embed: embeds.ElementAtOrDefault(index));

        await message.AddReactionsAsync(Emotes.DirectionEmotes.ToArray());

        while (true)
        {
            var reaction = await WaitForReactionAsync(message.Id, TimeSpan.FromSeconds(60), x => x.UserId == user.Id && Emotes.DirectionEmotes.Select(x => x.Name).Contains(x.Emote.Name));

            if (reaction == null || reaction.Emote.Name == Emotes.DirectionEmotes[2].Name)
                break;

            else if (reaction.Emote.Name == Emotes.DirectionEmotes[0].Name)
                index = 0;

            else if (reaction.Emote.Name == Emotes.DirectionEmotes[1].Name)
                index = (index > 0) ? index - 1 : index;

            else if (reaction.Emote.Name == Emotes.DirectionEmotes[3].Name)
                index = (index < embeds.Count() - 1) ? index + 1 : index;

            else if (reaction.Emote.Name == Emotes.DirectionEmotes[4].Name)
                index = embeds.Count() - 1;

            else if (reaction.Emote.Name == Emotes.DirectionEmotes[5].Name)
            {
                int result;
                var m = await WaitForMessageAsync(channel.Id, TimeSpan.FromSeconds(10), x => x.Author.Id == user.Id && int.TryParse(x.Content, out result));
                if (m != null)
                {
                    index = m.Content.ToInt32() - 1;
                    await m.DeleteAsync();
                }
            }

            await message.ModifyAsync(x => x.Embed = embeds.ElementAtOrDefault(index));
            await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
        }

        await message.DeleteAsync();
    }
}
