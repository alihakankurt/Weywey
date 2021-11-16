using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Information;

public partial class FunModule : ModuleBase<SocketCommandContext>
{
    [Name("Flip")]
    [Command("flip", RunMode = RunMode.Async)]
    [Alias("coin")]
    [Summary("Flips a coin.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task FlipCommand()
    {
        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = Context.User.ToString();
                footer.IconUrl = Context.User.GetAvatarUrl();
            })
            .WithDescription($"🪙 {((ProviderService.GetService<Random>().Next(0, 2) == 0) ? "Heads" : "Tails")}")
            .WithColor(Color.Orange).Build();

        await ReplyAsync(embed: embed);
    }
}
