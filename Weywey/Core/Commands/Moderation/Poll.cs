using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Weywey.Core.Constants;
using Weywey.Core.Services;

namespace Weywey.Core.Modules.Moderation
{
    public partial class ModerationModule : ModuleBase<SocketCommandContext>
    {
        [Name("Poll")]
        [Command("poll", RunMode = RunMode.Async)]
        [Summary("Creates a poll.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PollCommand([Summary("Duration of the poll from minutes. (max 2880)")] uint duration, [Summary("Question of the poll")] string question, [Summary("Options of the poll")] params string[] options)
        {
            if (options.Length < 2)
            {
                await ReplyAsync("You can pass at least 2 options to start a new poll!");
                return;
            }

            else if (options.Length > 10)
            {
                await ReplyAsync("You can't pass over 10 options!");
                return;
            }

            DateTime end = DateTime.UtcNow.AddSeconds(Math.Min(2880, Math.Max(1, duration)));
            var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = $"Created by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithTitle("New Poll!!")
                .WithDescription($"{question}\n\n{string.Join('\n', options.Select((x, i) => $"{Emotes.Numbers[i + 1]} {x}"))}")
                .AddField("Ends At", $"{end} UTC", false)
                .WithCurrentTimestamp().Build();
            var message = await ReplyAsync(embed: embed);
            for (int i = 0; i < options.Length; i++)
                await message.AddReactionAsync(Emotes.Numbers[i + 1]);
            ReactionService.AddPoll(Context.Guild.Id, Context.Channel.Id, message.Id, question, options, end);
        }
    }
}