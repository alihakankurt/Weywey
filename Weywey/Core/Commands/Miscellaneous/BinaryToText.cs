using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Miscellaneous;

public partial class MiscellaneousModule : ModuleBase<SocketCommandContext>
{
    [Name("Binary To Text")]
    [Command("btt", RunMode = RunMode.Async)]
    [Summary("Converts given binary to text.")]
    public async Task BinaryToTextCommand([Remainder][Summary("Binary string to convert")] string binary)
    {
        await ReplyAsync(binary.FromBinary().WithCodeBlock());
    }
}
