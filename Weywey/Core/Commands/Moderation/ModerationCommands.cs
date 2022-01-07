using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Moderation;

public static class ModerationCommands
{
    private static readonly DiscordSocketClient Client;
    private static readonly Random Random;
    private static readonly List<Giveaway> Giveaways;
    private static readonly Emoji Confetti;

    static ModerationCommands()
    {
        Client = ProviderService.GetService<DiscordSocketClient>();
        Random = new();
        Giveaways = new();
        Confetti = new("🎉");

        Client.MessageDeleted += Client_MessageDeleted; 
        Client.ButtonExecuted += Client_ButtonExecuted;
    }

    public static async Task KickAsync(ITextChannel channel, IUser user, SocketGuildUser target, string reason)
    {
        await target.KickAsync($"{reason}  -Actioned by {user}");
        await channel.SendMessageAsync($"**{target}** has kicked from server by {user.Mention}.");
    }

    public static async Task BanAsync(ITextChannel channel, IGuild guild, IUser user, IUser target, string reason)
    {
        await guild.AddBanAsync(target, 0, $"{reason}  > Actioned by {user}");
        await channel.SendMessageAsync($"`{target}` has banned from server by {user.Mention}.");
    }

    public static async Task UnbanAsync(ITextChannel channel, IGuild guild, IUser user, ulong target, string reason)
    {
        var ban = await guild.GetBanAsync(target);
        if (ban != null)
        {
            await guild.RemoveBanAsync(target, new RequestOptions { AuditLogReason = $"{reason}  > Actioned by {user}" });
            await channel.SendMessageAsync($"`{ban.User}` unbanned in server by {user.Mention}.");
        }
    }

    public static async Task ClearAsync(ITextChannel channel, uint amount)
    {
        amount = Math.Min(amount, 100);
        var messages = await channel.GetMessagesAsync((int)amount + 1).FlattenAsync();
        await channel.DeleteMessagesAsync(messages);
        var message = await channel.SendMessageAsync($"✅ {messages.Count() - 1} message(s) deleted.");
        await Task.Delay(1000);
        await message.DeleteAsync();
    }

    public static async Task SetNameAsync(ITextChannel channel, SocketGuildUser user, string nickname)
    {
        await user.ModifyAsync(x => x.Nickname = nickname);
        await channel.SendMessageAsync(string.IsNullOrEmpty(nickname) ? $"`{user}`'s nickname removed." : $"`{user}`'s nickname set to {nickname}");
    }

    public static async Task CreateReactionRoleAsync(ITextChannel channel, IGuild guild, IMessage lastMessage, string[] roleIds, GuildEmote[] emotes)
    {
        if (roleIds.Length != emotes.Length)
            return;

        await lastMessage.DeleteAsync();

        var embed = new EmbedBuilder()
            .WithTitle("Click to give yourself a role!")
            .WithColor(220, 20, 60)
            .WithCurrentTimestamp()
            .Build();

        var componentBuilder = new ComponentBuilder();
        for (int i = 0; i < roleIds.Length; i++)
            componentBuilder.WithButton(null, $"reaction-role-{guild.Id}-{roleIds[i]}", ButtonStyle.Secondary, emotes[i]);

        await channel.SendMessageAsync(embed: embed, components: componentBuilder.Build());
    }

    public static async Task StartGiveawayAsync(ITextChannel channel, IMessage lastMessage, IUser user, string prize)
    {
        await lastMessage.DeleteAsync();

        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = $"Hosted by {user.Username}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .WithTitle("New Giveaway!!")
            .WithDescription($"{user.Mention} started a new giveaway. Click on the {Confetti} button to join giveaway.")
            .AddField("Prize", prize, false)
            .AddField("Participants", 0, false)
            .WithColor(255, 20, 147)
            .WithCurrentTimestamp()
            .Build();

        var components = new ComponentBuilder()
            .WithButton("Join", "giveaway-join", ButtonStyle.Success, Confetti)
            .WithButton("End (Host)", "giveaway-end", ButtonStyle.Danger)
            .Build();

        var message = await channel.SendMessageAsync(embed: embed, components: components);
        Giveaways.Add(new(message.Id, user.Id, prize));
    }

    private static async Task Client_ButtonExecuted(SocketMessageComponent component)
    {
        if (component.Data.CustomId.StartsWith("giveaway"))
        {
            var giveaway = Giveaways.FirstOrDefault(x => x.MessageId == component.Message.Id);
            if (giveaway == null)
                await component.Message.DeleteAsync();

            else if (component.Data.CustomId == "giveaway-join")
            {
                if (giveaway.Participants.Contains(component.User.Id))
                    giveaway.Participants.Remove(component.User.Id);

                else
                    giveaway.Participants.Add(component.User.Id);

                var builder = component.Message.Embeds.First().ToEmbedBuilder();
                builder.Fields[1].WithValue(giveaway.Participants.Count);
                await component.UpdateAsync(x => x.Embed = builder.Build());
            }

            else if ((component.Data.CustomId == "giveaway-end") && (giveaway.UserId == component.User.Id))
            {
                var winner = (giveaway.Participants.Count == 0) ? null : Client.GetUser(giveaway.Participants[Random.Next(giveaway.Participants.Count)]);
                var embed = new EmbedBuilder()
                    .WithFooter(footer =>
                    {
                        footer.Text = $"Hosted by {component.User.Username}";
                        footer.IconUrl = component.User.GetAvatarUrl();
                    })
                    .WithTitle("Giveaway ended!!")
                    .WithDescription((winner == null) ? "No participant could be found!" : $"{winner.Mention} won the giveaway.")
                    .AddField("Prize", giveaway.Prize)
                    .WithColor(255, 20, 147)
                    .WithCurrentTimestamp()
                    .Build();

                await component.Message.DeleteAsync();
                await component.Channel.SendMessageAsync(embed: embed);
                Giveaways.Remove(giveaway);
                giveaway.Dispose();
            }
        }

        else if (component.Data.CustomId.StartsWith("reaction-role"))
        {
            var guild = Client.GetGuild(ulong.Parse(component.Data.CustomId[14..32]));
            var role = guild.GetRole(ulong.Parse(component.Data.CustomId[33..51]));
            if (role != null)
            {
                var user = component.User as SocketGuildUser;
                if (user.Roles.Contains(role))
                    await user.RemoveRoleAsync(role);
                
                else
                    await user.AddRoleAsync(role);
            }
        }

        await component.DeferAsync();
    }

    private static Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
    {
        var giveaway = Giveaways.FirstOrDefault(x => x.MessageId == arg1.Id);
        if (giveaway != null)
        {
            Giveaways.Remove(giveaway);
            giveaway.Dispose();
        }
        return Task.CompletedTask;
    }
}

public class Giveaway : IDisposable
{
    public ulong MessageId { get; set; }
    public ulong UserId { get; set; }
    public string Prize { get; set; }
    public List<ulong> Participants { get; set; }

    public Giveaway(ulong messageId, ulong userId, string prize)
    {
        MessageId = messageId;
        UserId = userId;
        Prize = prize;
        Participants = new();
    }

    public void Dispose()
    {
        MessageId = 0;
        UserId = 0;
        Prize = null;
        Participants = null;
    }
}
