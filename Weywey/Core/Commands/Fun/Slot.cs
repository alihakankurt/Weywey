using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using Weywey.Core.Entities;

namespace Weywey.Core.Commands.Fun
{
    public partial class FunModule : ModuleBase<SocketCommandContext>
    {
        [Name("Slot")]
        [Command("slot", RunMode = RunMode.Async)]
        [Summary("Play slot machine.")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task SlotCommand()
        {
            _slotMachine.Slot();

            string text = _slotMachine.MatchCount switch
            {
                2 => "Congrats, all matching!!",
                1 => "Match, you won.",
                0 => "Sorry, you lost.",
                _ => null
            };

            var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = Context.User.ToString();
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithDescription($"{_slotMachine.State}\n**{text}**")
                .WithColor(Color.Orange).Build();

            await ReplyAsync(embed: embed);
        }

        private SlotMachine _slotMachine = new SlotMachine("🍎", "🍊", "🍐", "🍋", "🍉", "🍇", "🍓", "🍒");
    }
}
