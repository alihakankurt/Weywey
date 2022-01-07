using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Miscellaneous;

public partial class MiscellaneousModule : ModuleBase<SocketCommandContext>
{
    [Name("Binary")]
    [Command("binary", RunMode = RunMode.Async)]
    [Summary("Converts value to binary.")]
    public async Task BinaryCommand([Remainder] string value)
        => await (int.TryParse(value, out int num)
                ? MiscellaneousCommands.DecimalToBinaryAsync(Context.Channel as ITextChannel, Context.User, num)
                : MiscellaneousCommands.TextToBinaryAsync(Context.Channel as ITextChannel, Context.User, value));

    [Name("Binary Quiz")]
    [Command("binary-quiz", RunMode = RunMode.Async)]
    [Summary("Asks a quesst to you.")]
    public async Task BinaryQuizCommand()
        => await MiscellaneousCommands.BinaryQuizAsync(Context.Channel as ITextChannel, Context.User);

    [Name("Wolfram Alpha")]
    [Command("wolfram", RunMode = RunMode.Async)]
    [Summary("Ask to Wolfram Alpha what you want.")]
    public async Task WolframAlphaCommand([Remainder] string query)
        => await MiscellaneousCommands.WolframAlphaAsync(Context.Channel as ITextChannel, Context.User, query);

    [Name("Color")]
    [Command("color", RunMode = RunMode.Async)]
    [Summary("Shows the color from given value.")]
    public async Task HexCommand([Remainder][Summary("The color in hex format or raw value")] Color color)
        => await MiscellaneousCommands.SendColorAsync(Context.Channel as ITextChannel, Context.User, color);
}
