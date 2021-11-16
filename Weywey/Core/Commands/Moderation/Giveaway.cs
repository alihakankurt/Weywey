using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weywey.Core.Constants;
using Weywey.Core.Services;

namespace Weywey.Core.Modules.Moderation;

public partial class ModerationModule : ModuleBase<SocketCommandContext>
{
    [Name("Giveaway")]
    [Command("giveaway", RunMode = RunMode.Async)]
    [Summary("Starts a new giveaway.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task GiveawayCommand([Summary("Duration of the giveaway from minutes (max 2880)")] uint duration, [Remainder][Summary("Prize to give")] string prize)
    {
        DateTime end = DateTime.UtcNow.AddMinutes(Math.Min(2880, Math.Max(1, duration)));
        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = $"Created by {Context.User}";
                footer.IconUrl = Context.User.GetAvatarUrl();
            })
            .WithTitle("New Giveaway!!")
            .WithDescription($"{Context.User.Mention} started a new giveaway. Hit the {Emotes.Confetti} to join giveaway.")
            .WithColor(Color.DarkRed)
            .AddField("Ends At", $"{end.ToShortDateString()} {end.ToShortTimeString()} UTC", false)
            .WithCurrentTimestamp().Build();

        var message = await ReplyAsync(embed: embed);
        await message.AddReactionAsync(Emotes.Confetti);
        GiveawayService.Add(Context.Guild.Id, Context.Channel.Id, message.Id, Context.User.Id, prize, end);
    }
}
