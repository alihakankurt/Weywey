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
        [Name("Slot")]
        [Command("slot", RunMode = RunMode.Async)]
        [Summary("Play slot machine.")]
        public async Task SlotCommand()
        {
            var random = ProviderService.GetService<Random>();
            string a = _slotEmotes[random.Next(0, _slotEmotes.Count - 1)];
            string b = _slotEmotes[random.Next(0, _slotEmotes.Count - 1)];
            string c = _slotEmotes[random.Next(0, _slotEmotes.Count - 1)];

            string text;

            if (a == b && b == c && a == c)
                text = "Congrats, all matching!!";

            else if (a == b || b == c || a == c)
                text = "Two match, you won.";

            else
                text = "Sorry, you lost.";

            var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = Context.User.ToString();
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithDescription($"[ {a} {b} {c} ]\n**{text}**")
                .WithColor(Color.Orange).Build();

            await ReplyAsync(embed: embed);
        }

        private List<string> _slotEmotes = new List<string> { "🍎", "🍊", "🍐", "🍋", "🍉", "🍇", "🍓", "🍒" };
    }
}
