using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Fun
{
    public partial class FunModule : ModuleBase<SocketCommandContext>
    {
        [Name("Beer")]
        [Command("beer", RunMode = RunMode.Async)]
        [Summary("Drink beer with friends.")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task BeerCommand([Remainder] [Summary("User to offer beer")] SocketGuildUser user = null)
        {
            if (user == null || user.Id == Context.User.Id)
            {
                await ReplyAsync($"**{Context.User.Username}**: paaaarty!🎉🍺");
            }

            else if (user.Id == Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("*drinks with you* 🍻");
            }

            else if (user.IsBot)
            {
                await ReplyAsync($"I would love to offer **{Context.User.Username}**, but i don't think a bot will answer you :/");
            }

            else
            {
                var message = await ReplyAsync($"**{user.Username}**, you got an offer from **{Context.User.Username}** 🍺 to you.");
                await message.AddReactionAsync(new Emoji("🍺"));
                var reaction = await ReactionService.WaitForReactionAsync(message.Id, TimeSpan.FromSeconds(30), x => x.User.Value.Id == user.Id && x.Emote.Name == "🍺");

                if (reaction == null)
                    await message.ModifyAsync(x => x.Content = $"Sorry **{Context.User.Username}**, **{user.Username}** don't want to drink with you 😢");

                else
                    await message.ModifyAsync(x => x.Content = $"***{user.Username}*** and ***{Context.User.Username}*** is enjoying a good beer 🍻");
            }
        }
    }
}
