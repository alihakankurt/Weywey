﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Weywey.Core.Commands.Fun
{
    public partial class InformationModule : ModuleBase<SocketCommandContext>
    {
        [Name("User Information")]
        [Command("userinfo", RunMode = RunMode.Async)]
        [Summary("Shows a user's information.")]
        public async Task UserInfoCommand([Remainder] [Summary("Whose information will be shown")] SocketGuildUser user = null)
        {
            user = user ?? Context.User as SocketGuildUser;
            var topRole = user.Roles.OrderByDescending(x => x.Position).First();

            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author.Name = user.ToString();
                    author.IconUrl = user.GetAvatarUrl();
                })
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .AddField("Id", user.Id, false)
                .AddField("Is bot?", user.IsBot, false)
                .AddField("Status", user.Status, false)
                .AddField("Activity", (user.Activity == null ? "No activity" : $"{user.Activity.Type} {user.Activity.Name}"), false)
                .AddField("Top Role", topRole.Mention)
                .AddField("Created At", user.CreatedAt, false)
                .AddField("Joined At", user.JoinedAt, false)
                .WithColor(topRole.Color)
                .WithCurrentTimestamp().Build();

            await ReplyAsync(embed: embed);
        }
    }
}
