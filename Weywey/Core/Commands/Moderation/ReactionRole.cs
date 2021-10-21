using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Weywey.Core.Services;

namespace Weywey.Core.Modules.Moderation
{
    public partial class ModerationModule : ModuleBase<SocketCommandContext>
    {
        [Name("Reaction Role")]
        [Command("reaction-role", RunMode = RunMode.Async)]
        [Summary("Creates a reaction role message.")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task ReactionRoleCommand([Remainder] [Summary("The reaction role count")] int count)
        {
            var dictionary = new Dictionary<SocketRole, SocketReaction>();

            await ReplyAsync($"Mention {count} roles and react for each one.");
            for (int i = 0; i < count; i++)
            {
                var msg = await ReactionService.WaitForMessageAsync(Context.Channel.Id, TimeSpan.FromSeconds(30), x => x.Author.Id == Context.User.Id && x.MentionedRoles.Count == 1);
                if (msg == null)
                {
                    await ReplyAsync("Timed out.");
                    return;
                }

                var reaction = await ReactionService.WaitForReactionAsync(msg.Id, TimeSpan.FromSeconds(10), x => x.UserId == Context.User.Id);
                if (reaction == null)
                {
                    await ReplyAsync("Timed out.");
                    return;
                }
                dictionary.Add(msg.MentionedRoles.First(), reaction);
            }

            var embed = new EmbedBuilder()
                .WithTitle("React to give yourself a role.")
                .WithDescription(string.Join("\n", dictionary.Select(x => $"{x.Value.Emote} {x.Key.Mention}")))
                .WithCurrentTimestamp().Build();

            var message = await ReplyAsync(embed: embed);
            await message.AddReactionsAsync(dictionary.Select(x => x.Value.Emote).ToArray());
            ReactionService.AddReactionRole(message.Id, dictionary.ToDictionary(x => x.Value.Emote.ToString(), x =>  x.Key.Id));
        }
    }
}