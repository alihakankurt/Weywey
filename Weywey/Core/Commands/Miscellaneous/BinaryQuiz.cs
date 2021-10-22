using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Miscellaneous
{
    public partial class MiscellaneousModule : ModuleBase<SocketCommandContext>
    {
        [Name("Binary Quiz")]
        [Command("binary-quiz", RunMode = RunMode.Async)]
        [Summary("Asks a quesst to you.")]
        public async Task BinaryQuizCommand()
        {
            int value = ProviderService.GetService<Random>().Next(0, 256);
            string binary = Convert.ToString(value, 2);
            var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithDescription($"Write `{value}` with bits in 15 seconds.")
                .WithColor(Color.Purple).Build();

            await ReplyAsync(embed: embed);
            var response = (await ReactionService.WaitForMessageAsync(Context.Channel.Id, TimeSpan.FromSeconds(15), x => x.Author.Id == Context.User.Id))?.Content;
            if (response == binary)
                await ReplyAsync("Correct.");

            else
                await ReplyAsync($"Answer was `{binary}`.");
        }
    }
}
