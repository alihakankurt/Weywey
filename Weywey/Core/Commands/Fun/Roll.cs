﻿using Discord;
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
        [Name("Roll")]
        [Command("roll", RunMode = RunMode.Async)]
        [Alias("dice")]
        [Summary("Rolls a dice.")]
        public async Task RollCommand()
        {
            var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = Context.User.ToString();
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithDescription($"🎲 {ProviderService.GetService<Random>().Next(1, 7)}")
                .WithColor(Color.Orange).Build();

            await ReplyAsync(embed: embed);
        }
    }
}