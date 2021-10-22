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
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ReactionRoleCommand([Summary("The title for reaction roles.")] string title, [Remainder] [Summary("The reaction role count")] int count)
        {
            var dictionary = new Dictionary<SocketRole, SocketReaction>();

            var message = await ReplyAsync($"Mention {count} roles and react for each one.");
            for (int i = 0; i < count; i++)
            {
                var msg = await ReactionService.WaitForMessageAsync(Context.Channel.Id, TimeSpan.FromSeconds(15), x => x.Author.Id == Context.User.Id && x.MentionedRoles.Count == 1);
                if (msg == null)
                {
                    await message.DeleteAsync();
                    return;
                }

                var reaction = await ReactionService.WaitForReactionAsync(msg.Id, TimeSpan.FromSeconds(15), x => x.UserId == Context.User.Id);
                if (reaction == null)
                {
                    await message.DeleteAsync();
                    return;
                }
                await msg.DeleteAsync();
                dictionary.Add(msg.MentionedRoles.First(), reaction);
            }
            await message.DeleteAsync();

            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription("React to give yourself a role.\n\n" + string.Join("\n", dictionary.Select(x => $"{x.Value.Emote} {x.Key.Mention}")))
                .WithCurrentTimestamp().Build();
            message = await ReplyAsync(embed: embed);
            await message.AddReactionsAsync(dictionary.Select(x => x.Value.Emote).ToArray());
            ReactionService.AddReactionRole(message.Id, dictionary.ToDictionary(x => x.Value.Emote.ToString(), x =>  x.Key.Id));
        }
    }
}