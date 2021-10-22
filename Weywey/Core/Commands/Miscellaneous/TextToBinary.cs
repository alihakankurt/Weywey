using Discord.Commands;
using System;
using System.Threading.Tasks;
using Weywey.Core.Extensions;

namespace Weywey.Core.Commands.Miscellaneous
{
    public partial class MiscellaneousModule : ModuleBase<SocketCommandContext>
    {
        [Name("Text To Binary")]
        [Command("ttb", RunMode = RunMode.Async)]
        [Summary("Converts given text to binary.")]
        public async Task TextToBinaryCommand([Remainder] [Summary("The content to reply as binary")] string text)
        {
            await ReplyAsync(text.ToBinary().WithCodeBlock());
        }
    }
}
