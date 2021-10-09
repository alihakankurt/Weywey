using System;
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
        [Name("Emote Information")]
        [Command("emoteinfo", RunMode = RunMode.Async)]
        [Summary("Shows a guild emote's information.")]
        public async Task EmoteInfoCommand([Summary("What information will be shown")] GuildEmote emote)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author.Name = emote.Name;
                    author.IconUrl = emote.Url;
                })
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .AddField("Id", emote.Id, false)
                .AddField("Created At", emote.CreatedAt, false)
                .AddField("Creator Id", emote.CreatorId, false)
                .AddField("Is animated?", emote.Animated, false)
                .WithCurrentTimestamp().Build();
            await ReplyAsync(embed: embed);
        }
    }
}
