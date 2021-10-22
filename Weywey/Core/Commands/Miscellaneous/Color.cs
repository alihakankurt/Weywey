using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Miscellaneous
{
    public partial class MiscellaneousModule : ModuleBase<SocketCommandContext>
    {
        [Name("Color")]
        [Command("color", RunMode = RunMode.Async)]
        [Summary("Shows the color from given input.")]
        public async Task HexCommand([Remainder] [Summary("The color in hex format or raw value")] Color color)
        {
            var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithDescription($"Hex: {color}\nR: {color.R}\nG: {color.G}\nB: {color.B}")
                .WithColor(color)
                .WithImageUrl("https://some-random-api.ml/canvas/colorviewer?hex=" + color.ToString().Trim('#')).Build();

            await ReplyAsync(embed: embed);
        }
    }
}
