using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Weywey.Core.Constants;
using Weywey.Core.Entities;
using Weywey.Core.Extensions;

namespace Weywey.Core.Services
{
    public static class ReactionService
    {
        private static string _reactionPath = "reaction-roles.json";
        private static string _pollPath = "polls.json";
        private static DiscordSocketClient _client { get; set; }
        private static List<WaitlistReactionItem> _waitlistReactions { get; set; }
        private static List<WaitlistMessageItem> _waitlistMessages { get; set; }
        private static List<ReactionRoleItem> _reactionRoles { get; set; }
        private static List<PollItem> _polls { get; set; }
        public static void RunService()
        {
            _client = ProviderService.GetService<DiscordSocketClient>();
            _waitlistReactions = new List<WaitlistReactionItem>();
            _waitlistMessages = new List<WaitlistMessageItem>();
            _reactionRoles = DataService.Get<List<ReactionRoleItem>>(_reactionPath);
            _polls = DataService.Get<List<PollItem>>(_pollPath);

            _client.ReactionAdded += ReactionService_ReactionAdded;
            _client.ReactionRemoved += ReactionService_ReactionRemoved;
            _client.MessageReceived += ReactionService_MessageReceived;
            _client.MessageDeleted += ReactionService_MessageDeleted;
            _client.RoleDeleted += ReactionService_RoleDeleted;
        }

        public static async Task CompletePolls()
        {
            while (_client.ConnectionState == ConnectionState.Connected)
            {
                await Task.Run(async () =>
                {
                    foreach (var item in _polls)
                        if (DateTime.UtcNow > item.End)
                        {
                            var channel = _client.GetGuild(item.GuildId).GetTextChannel(item.ChannelId);
                            var message = await channel.GetMessageAsync(item.MessageId);
                            var reactions = message.Reactions.Where(x => Emotes.Numbers.Any(n => n.Name == x.Key.Name)).OrderByDescending(x => x.Value.ReactionCount).ToList();
                            var most = reactions.FirstOrDefault().Value.ReactionCount - 1;
                            reactions.RemoveAll(x => x.Value.ReactionCount - 1 != most);
                            string content;
                            if (most == 0)
                                content = $"No votes found for poll `({item.MessageId})`.";

                            else if (reactions.Count == 1)
                                content = $"Option {reactions[0].Key.Name} won the poll `({item.MessageId})` with `{most}` votes.";

                            else if (reactions.Count == 2)
                                content = $"Options {reactions[0].Key.Name} and {reactions[1].Key.Name} has tied the poll `({item.MessageId})` with `{most}` votes.";

                            else
                                content = $"Too many options has tied for poll `({item.MessageId})`.";

                            await channel.SendMessageAsync(content + ((most == 31) ? " (sjsj)" : ((most == 69) ? " (asdasd)" : "")));
                            item.Completed = true;
                        }

                    RemovePoll(x => x.Completed);
                    await Task.Delay(30000);
                });
            }
        }

        private static async Task ReactionService_ReactionAdded(Cacheable<IUserMessage, ulong> param, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot || (channel as SocketTextChannel) == null)
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

                    else if (item.Filter(reaction))
                        item.Reaction = reaction;
        }

        private static async Task ReactionService_ReactionRemoved(Cacheable<IUserMessage, ulong> param, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot || (channel as SocketTextChannel) == null)
                return;

            var message = await param.GetOrDownloadAsync();

            foreach (var item in _reactionRoles)
                if (item.MessageId == message.Id)
                    if (item.Roles.ContainsKey(reaction.Emote.ToString()))
                        await (reaction.User.Value as SocketGuildUser).RemoveRoleAsync(item.Roles[reaction.Emote.ToString()]);
        }

        private static Task ReactionService_MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot || (message.Channel as SocketTextChannel) == null)
                return Task.CompletedTask;

            foreach (var item in _waitlistMessages.Where(x => !x.Expired))
                if (item.ChannelId == message.Channel.Id)
                    if (item.Filter == null)
                        item.Message = message;

                    else if (item.Filter(message))
                        item.Message = message;

            return Task.CompletedTask;
        }

        private static Task ReactionService_MessageDeleted(Cacheable<IMessage, ulong> param, ISocketMessageChannel channel)
        {
            if ((channel as SocketTextChannel) == null)
                return Task.CompletedTask;

            RemoveReactionRole(x => x.MessageId == param.Id);
            RemovePoll(x => x.MessageId == param.Id);

            return Task.CompletedTask;
        }

        private static Task ReactionService_RoleDeleted(SocketRole role)
        {
            foreach (var item in _reactionRoles)
                if (item.Roles.ContainsValue(role.Id))
                    item.Roles.Remove(item.Roles.First(x => x.Value == role.Id).Key);

            return Task.CompletedTask;
        }

        public static async Task<SocketReaction> WaitForReactionAsync(ulong messageId, TimeSpan duration, Predicate<SocketReaction> filter = null)
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

        public static async Task<SocketMessage> WaitForMessageAsync(ulong channelId, TimeSpan duration, Predicate<SocketMessage> filter = null)
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
            DataService.Save(_reactionPath, _reactionRoles);
        }

        public static void RemoveReactionRole(Predicate<ReactionRoleItem> filter)
        {
            int count = _reactionRoles.RemoveAll(filter);
            if (count > 0)
                DataService.Save(_reactionPath, _reactionRoles);
        }

        public static void AddPoll(ulong guildId, ulong channelId, ulong messageId, string question, string[] options, DateTime end)
        {
            var poll = new PollItem
            {
                GuildId = guildId,
                ChannelId = channelId,
                MessageId = messageId,
                Question = question,
                Options = options,
                End = end
            };
            _polls.Add(poll);
            DataService.Save(_pollPath, _polls);
        }

        public static void RemovePoll(Predicate<PollItem> filter)
        {
            int count = _polls.RemoveAll(filter);
            if (count > 0)
                DataService.Save(_pollPath, _polls);
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
}
