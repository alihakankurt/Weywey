using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Fun
{
    public partial class FunModule : ModuleBase<SocketCommandContext>
    {
        [Name("Flip")]
        [Command("flip", RunMode = RunMode.Async)]
        [Alias("coin")]
        [Summary("Flips a coin.")]
        public async Task FlipCommand()
        {
            string text = ProviderService.GetService<Random>().Next(0, 2) switch
            {
                0 => "Heads",
                1 => "Tails",
                _ => null
            };

            var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = Context.User.ToString();
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithDescription($"🪙 {text}")
                .WithColor(Color.Orange).Build();

            var m = await ReplyAsync(embed: embed);
        }
    }
}
