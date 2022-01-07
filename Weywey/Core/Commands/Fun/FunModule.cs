using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Fun;

public partial class FunModule : ModuleBase<SocketCommandContext>
{
    [Name("Flip")]
    [Command("flip", RunMode = RunMode.Async)]
    [Alias("coin")]
    [Summary("Flips a coin.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task FlipCommand()
        => await FunCommands.FlipAsync(Context.Channel as ITextChannel, Context.User);

    [Name("Roll")]
    [Command("roll", RunMode = RunMode.Async)]
    [Alias("dice")]
    [Summary("Rolls a dice.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task RollCommand()
        => await FunCommands.RollAsync(Context.Channel as ITextChannel, Context.User);

    [Name("Slot")]
    [Command("slot", RunMode = RunMode.Async)]
    [Summary("Play slot machine.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task SlotCommand()
        => await FunCommands.SlotAsync(Context.Channel as ITextChannel, Context.User);

    [Name("Owoify")]
    [Command("owoify", RunMode = RunMode.Async)]
    [Summary("Owoify your message.")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public async Task OwoifyCommand([Remainder] string content)
        => await FunCommands.OwoifyAsync(Context.Channel as ITextChannel, Context.User, content);
}
