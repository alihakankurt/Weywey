using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Weywey.Core.Commands.Information;

public partial class InformationModule : ModuleBase<SocketCommandContext>
{
    [Name("Emote Information")]
    [Command("emoteinfo", RunMode = RunMode.Async)]
    [Summary("Shows a guild emote's information.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task EmoteInfoCommand([Remainder][Summary("What information will be shown")] GuildEmote emote)
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
