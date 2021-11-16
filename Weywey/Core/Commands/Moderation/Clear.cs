using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Moderation;

public partial class ModerationModule : ModuleBase<SocketCommandContext>
{
    [Name("Clear")]
    [Command("clear", RunMode = RunMode.Async)]
    [Alias("purge")]
    [Summary("Deletes spesific amount of messages.")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task ClearCommand([Summary("Limit for scanning text channel")] uint scan, [Remainder][Summary("The author of the messages")] SocketGuildUser target = null)
    {
        await Context.Message.DeleteAsync();
        scan = Math.Min(scan, 100);
        var messages = (await Context.Channel.GetMessagesAsync((int)scan).FlattenAsync()).Where(m => (target == null) || (m.Author.Id == target.Id));
        await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
        var message = await ReplyAsync($"✅ {messages.Count()} message(s) deleted.");
        await Task.Delay(1000);
        await message.DeleteAsync();
    }
}
