using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Weywey.Core.Extensions;

namespace Weywey.Core.Services
{
    public static class ReactionService
    {
        private static string _path = "reaction-roles.json";
        private static DiscordSocketClient _client { get; set; }
        private static List<WaitlistReactionItem> _waitlistReactions { get; set; }
        private static List<WaitlistMessageItem> _waitlistMessages { get; set; }
        private static List<ReactionRoleItem> _reactionRoles { get; set; }
        private static List<IEmote> _emotes { get; set; }
        public static void RunService()
        {
            _waitlistReactions = new List<WaitlistReactionItem>();
            _waitlistMessages = new List<WaitlistMessageItem>();
            _emotes = new List<IEmote>() { new Emoji("⏪"), new Emoji("◀️"), new Emoji("⏹"), new Emoji("▶️"), new Emoji("⏩"), new Emoji("🔢") };

            if (!File.Exists(_path))
                File.Create(_path).Close();

            using (var reader = new StreamReader(_path))
            {
                _reactionRoles = JsonConvert.DeserializeObject<List<ReactionRoleItem>>(reader.ReadToEnd()) ?? new List<ReactionRoleItem>();
            }

            _client = ProviderService.GetService<DiscordSocketClient>();
            _client.ReactionAdded += ReactionService_ReactionAdded;
            _client.ReactionRemoved += ReactionService_ReactionRemoved;
            _client.MessageReceived += ReactionService_MessageReceived;
            _client.RoleDeleted += ReactionService_RoleDeleted;
        }

        private static Task ReactionService_RoleDeleted(SocketRole role)
        {
            foreach (var item in _reactionRoles)
                if (item.Roles.ContainsValue(role.Id))
                    item.Roles.Remove(item.Roles.First(x => x.Value == role.Id).Key);

            return Task.CompletedTask;
        }

        private static async Task ReactionService_ReactionRemoved(Cacheable<IUserMessage, ulong> param, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if ((channel as SocketTextChannel) == null)
                return;

            var message = await param.GetOrDownloadAsync();

            foreach (var item in _reactionRoles)
                if (item.MessageId == message.Id)
                    if (item.Roles.ContainsKey(reaction.Emote.ToString()))
                        await (reaction.User.Value as SocketGuildUser).RemoveRoleAsync(item.Roles[reaction.Emote.ToString()]);
        }

        private static async Task ReactionService_ReactionAdded(Cacheable<IUserMessage, ulong> param, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if ((channel as SocketTextChannel) == null)
                return;

            var message = await param.GetOrDownloadAsync();

            foreach (var item in _reactionRoles)
                if (item.MessageId == message.Id)
                    if (item.Roles.ContainsKey(reaction.Emote.ToString()))
                        await (reaction.User.Value as SocketGuildUser).AddRoleAsync(item.Roles[reaction.Emote.ToString()]);

            foreach (var item in _waitlistReactions.Where(x => !x.Expired))
                if (item.MessageId == message.Id)
                    if (item.Filter == null)
                        item.Reaction = reaction;

                    else
                        if (item.Filter(reaction))
                        item.Reaction = reaction;
        }

        private static Task ReactionService_MessageReceived(SocketMessage message)
        {
            if ((message.Channel as SocketTextChannel) == null)
                return Task.CompletedTask;

            foreach (var item in _waitlistMessages.Where(x => !x.Expired))
                if (item.ChannelId == message.Channel.Id)
                    if (item.Filter == null)
                        item.Message = message;

                    else
                        if (item.Filter(message))
                        item.Message = message;

            return Task.CompletedTask;
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

        public static void AddReactionRole(ulong messageId, Dictionary<string, ulong> roles)
        {
            _reactionRoles.Add(new ReactionRoleItem
            {
                MessageId = messageId,
                Roles = roles
            });

            using (var writer = new StreamWriter(_path))
            {
                writer.Write(JsonConvert.SerializeObject(_reactionRoles));
            }
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
                    index = index > 0 ? index - 1 : index;

                else if (reaction.Emote.Name == _emotes[3].Name)
                    index = index < embeds.Count() - 1 ? index + 1 : index;

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

    public class ReactionRoleItem
    {
        public ulong MessageId { get; set; }
        public Dictionary<string, ulong> Roles { get; set; }
    }
}
