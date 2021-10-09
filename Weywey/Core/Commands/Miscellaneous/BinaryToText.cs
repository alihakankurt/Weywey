﻿using Discord;
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
        [Name("Binary To Text")]
        [Command("btt", RunMode = RunMode.Async)]
        [Summary("Converts given binary to text.")]
        public async Task BinaryToTextCommand([Remainder] string binary)
        {
            await ReplyAsync(binary.FromBinary().WithCodeBlock());
        }
    }
}
