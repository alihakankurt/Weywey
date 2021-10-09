using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Weywey.Core.Extensions;

namespace Weywey.Core.Commands.Miscellaneous
{
    public partial class MiscellaneousModule : ModuleBase<SocketCommandContext>
    {
        [Name("Text To Binary")]
        [Command("ttb", RunMode = RunMode.Async)]
        [Summary("Converts given text to binary.")]
        public async Task TextToBinaryCommand([Remainder] string text)
        {
            await ReplyAsync(text.ToBinary().WithCodeBlock());
        }
    }
}
