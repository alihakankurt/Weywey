using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weywey.Core.Constants;
using Weywey.Core.Entities;

namespace Weywey.Core.Services;

public static class PollService
{
    private static readonly string Path = "Polls.json";
    private static List<PollItem> Polls { get; set; }
    private static DiscordSocketClient Client { get; set; }

    public static void RunService()
    {
        Polls = DataService.Get<List<PollItem>>(Path);
        Client = ProviderService.GetService<DiscordSocketClient>();

        Client.Ready += Client_Ready;
        Client.MessageDeleted += Client_MessageDeleted;
    }

    public static void Add(ulong guildId, ulong channelId, ulong messageId, string question, string[] options, DateTime end)
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
        Polls.Add(poll);
        DataService.Save(Path, Polls);
    }

    public static void Remove(Predicate<PollItem> filter)
    {
        if (Polls.RemoveAll(filter) > 0)
            DataService.Save(Path, Polls);
    }

    private static Task Client_Ready()
    {
        var t = new Task(async () =>
        {
            await PollHandler();
        });
        t.Start();
        return Task.CompletedTask;
    }

    private static async Task PollHandler()
    {
        while (Client.ConnectionState == ConnectionState.Connected)
        {
            foreach (var item in Polls)
                if (DateTime.UtcNow > item.End)
                {
                    var channel = Client.GetGuild(item.GuildId).GetTextChannel(item.ChannelId);
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

                    await channel.SendMessageAsync(content);
                    item.Completed = true;
                }

            Remove(x => x.Completed);
            await Task.Delay(30000);
        }
    }

    private static Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg, ISocketMessageChannel channel)
    {
        if ((channel as SocketTextChannel) == null)
            return Task.CompletedTask;

        Remove(x => x.MessageId == arg.Id);
        return Task.CompletedTask;
    }
}
