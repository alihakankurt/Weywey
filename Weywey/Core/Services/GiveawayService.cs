using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weywey.Core.Constants;
using Weywey.Core.Entities;

namespace Weywey.Core.Services;

public class GiveawayService
{
    private static readonly string Path = "Giveaways.json";
    private static List<GiveawayItem> Giveaways { get; set; }
    private static DiscordSocketClient Client { get; set; }

    public static void RunService()
    {
        Giveaways = DataService.Get<List<GiveawayItem>>(Path);
        Client = ProviderService.GetService<DiscordSocketClient>();

        Client.Ready += Client_Ready;
        Client.MessageDeleted += Client_MessageDeleted;
    }

    public static void Add(ulong guildId, ulong channelId, ulong messageId, ulong userId, string prize, DateTime end)
    {
        Giveaways.Add(new GiveawayItem
        {
            GuildId = guildId,
            ChannelId = channelId,
            MessageId = messageId,
            UserId = userId,
            Prize = prize,
            End = end
        });
        DataService.Save(Path, Giveaways);
    }

    public static void Remove(Predicate<GiveawayItem> filter)
    {
        if (Giveaways.RemoveAll(filter) > 0)
            DataService.Save(Path, Giveaways);
    }

    private static Task Client_Ready()
    {
        var t = new Task(async () =>
        {
            await GiveawayHandler();
        });
        t.Start();
        return Task.CompletedTask;
    }

    private static async Task GiveawayHandler()
    {
        while (Client.ConnectionState == ConnectionState.Connected)
        {
            foreach (var giveaway in Giveaways)
                if (DateTime.UtcNow > giveaway.End)
                {
                    var channel = Client.GetGuild(giveaway.GuildId).GetTextChannel(giveaway.ChannelId);
                    var message = await channel.GetMessageAsync(giveaway.MessageId);
                    var user = Client.GetUser(giveaway.UserId);

                    var users = (await message.GetReactionUsersAsync(Emotes.Confetti, 1000).FlattenAsync()).Where(user => user.Id != Client.CurrentUser.Id).ToList();
                    if (users.Count == 0)
                    {
                        await channel.SendMessageAsync($"Couldn't found any participant to choose a winner for giveaway.`({giveaway.MessageId})`");
                    }

                    else
                    {
                        var winner = users.Choose();
                        await channel.SendMessageAsync($"Congrats, {winner.Mention}! You won the giveaway `({giveaway.MessageId})`. Contact with the {user.Mention} to get your prize.");
                    }
                    giveaway.Completed = true;
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
