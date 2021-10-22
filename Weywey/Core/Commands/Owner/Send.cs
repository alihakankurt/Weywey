using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Owner
{
    public partial class OwnerModule : ModuleBase<SocketCommandContext>
    {
        [Name("Send")]
        [Command("send")]
        [Summary("Sends message to target channel.")]
        [RequireOwner]
        public async Task SendCommand([Summary("Channel id")] ulong id, [Remainder] [Summary("The message to sent")] string message)
        {
            var channel = Context.Client.GetChannel(id);

            if (channel == null || !(channel is IMessageChannel))
                if (Context.Client.GetUser(id) is var user && user != null && !user.IsBot)
                    await user.SendMessageAsync(message);
                
                else
                    await ReplyAsync("Channel not found with this id!");

            else
                await (channel as IMessageChannel).SendMessageAsync(message);
        }
    }
}
