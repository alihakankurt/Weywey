using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Weywey.Core.Extensions;

namespace Weywey.Core.Services
{
    public static class ReactionService
    {
        private static List<WaitlistReactionItem> _waitlistReactions { get; set; }
        private static List<WaitlistMessageItem> _waitlistMessages { get; set; }
        private static List<IEmote> _emotes { get; set; }
        public static void RunService()
        {
            _waitlistReactions = new List<WaitlistReactionItem>();
            _waitlistMessages = new List<WaitlistMessageItem>();
            _emotes = new List<IEmote>() { new Emoji("⏪"), new Emoji("◀️"), new Emoji("⏹"), new Emoji("▶️"), new Emoji("⏩"), new Emoji("🔢") };

            ProviderService.GetService<DiscordSocketClient>().ReactionAdded += ReactionService_ReactionAdded;
            ProviderService.GetService<DiscordSocketClient>().MessageReceived += ReactionService_MessageReceived;
        }

        private static async Task ReactionService_ReactionAdded(Cacheable<IUserMessage, ulong> param, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await param.GetOrDownloadAsync();

            foreach (var item in _waitlistReactions.Where(x => !x.Expired))
                if (item.MessageId == message.Id)
                    if (item.Filter == null)
                        item.Reaction = reaction;

                    else
                        if (item.Filter(reaction))
                        item.Reaction = reaction;
        }

        public static async Task<SocketReaction> WaitForReactionAsync(ulong messageId, TimeSpan duration, Expression<Func<SocketReaction, bool>> filter = null)
        {
            var item = new WaitlistReactionItem(messageId, duration, filter);

            _waitlistReactions.Add(item);

            while (item.Reaction == null && !item.Expired)
            {
                await Task.Delay(1);
            }

            _waitlistReactions.Remove(item);

            return item.Reaction;
        }

        private static Task ReactionService_MessageReceived(SocketMessage message)
        {
            foreach (var item in _waitlistMessages.Where(x => !x.Expired))
                if (item.ChannelId == message.Channel.Id)
                    if (item.Filter == null)
                        item.Message = message;

                    else
                        if (item.Filter(message))
                        item.Message = message;

            return Task.CompletedTask;
        }

        public static async Task<SocketMessage> WaitForMessageAsync(ulong channelId, TimeSpan duration, Expression<Func<SocketMessage, bool>> filter = null)
        {
            var item = new WaitlistMessageItem(channelId, duration, filter);

            _waitlistMessages.Add(item);

            while (item.Message == null && !item.Expired)
            {
                await Task.Delay(1);
            }

            _waitlistMessages.Remove(item);

            return item.Message;
        }

        public static async Task PaginateAsync(IMessageChannel channel, IUser user, IEnumerable<Embed> embeds)
        {
            if (embeds.Count() < 1)
                return;

            int index = 0;
            DateTime start = DateTime.UtcNow;

            var message = await channel.SendMessageAsync(embed: embeds.ElementAtOrDefault(index));

            await message.AddReactionsAsync(_emotes.ToArray());

            while (true)
            {
                var reaction = await WaitForReactionAsync(message.Id, TimeSpan.FromSeconds(60), x => x.UserId == user.Id && _emotes.Select(x => x.Name).Contains(x.Emote.Name));

                if (reaction == null || reaction.Emote.Name == _emotes[2].Name)
                    break;

                else if (reaction.Emote.Name == _emotes[0].Name)
                    index = 0;

                else if (reaction.Emote.Name == _emotes[1].Name)
                    index = index > 0 ? index-- : index;

                else if (reaction.Emote.Name == _emotes[3].Name)
                    index = index < embeds.Count() - 1 ? index++ : index;

                else if (reaction.Emote.Name == _emotes[4].Name)
                    index = embeds.Count() - 1;

                else if (reaction.Emote.Name == _emotes[5].Name)
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

    public class WaitlistReactionItem
    {
        public WaitlistReactionItem(ulong messageId, TimeSpan duration, Expression<Func<SocketReaction, bool>> filter)
        {
            MessageId = messageId;
            Duration = duration;
            Filter = filter == null ? null : filter.Compile();
            CreatedAt = DateTime.UtcNow;
        }

        public ulong MessageId { get; set; }
        public TimeSpan Duration { get; set; }
        public Func<SocketReaction, bool> Filter { get; set; }
        public SocketReaction Reaction { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Expired
            => DateTime.UtcNow.Subtract(CreatedAt).TotalSeconds >= Duration.TotalSeconds;
    }

    public class WaitlistMessageItem
    {
        public WaitlistMessageItem(ulong channelId, TimeSpan duration, Expression<Func<SocketMessage, bool>> filter)
        {
            ChannelId = channelId;
            Duration = duration;
            Filter = filter == null ? null : filter.Compile();
            CreatedAt = DateTime.UtcNow;
        }

        public ulong ChannelId { get; set; }
        public TimeSpan Duration { get; set; }
        public Func<SocketMessage, bool> Filter { get; set; }
        public SocketMessage Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Expired
            => DateTime.UtcNow.Subtract(CreatedAt).TotalSeconds >= Duration.TotalSeconds;
    }
}
