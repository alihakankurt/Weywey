using Discord;
using System;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Fun;

public static class FunCommands
{
    private static readonly Random Random;
    private static readonly string[] Fruits;
    private static readonly string[] OwoFaces;

    static FunCommands()
    {
        Random = new();
        Fruits = new string[] { "🍎", "🍊", "🍐", "🍋", "🍉", "🍇", "🍓", "🍒" };
        OwoFaces = new string[] { " (・`ω´・) ", " ;;w;; ", " owo ", " UwU ", " >w< ", " ^w^ ", " >.< " };
    }

    public static async Task FlipAsync(ITextChannel channel, IUser user)
    {
        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = user.Username;
                footer.IconUrl = user.GetAvatarUrl();
            })
            .WithDescription($"🪙 {((Random.Next(2) == 0) ? "Heads" : "Tails")}")
            .WithColor(Color.Orange)
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task RollAsync(ITextChannel channel, IUser user)
    {
        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = user.ToString();
                footer.IconUrl = user.GetAvatarUrl();
            })
            .WithDescription($"🎲 {Random.Next(6) + 1}")
            .WithColor(Color.Orange)
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task SlotAsync(ITextChannel channel, IUser user)
    {
        int x = Random.Next(8);
        int y = Random.Next(8);
        int z = Random.Next(8);
        bool match = x == y || x == z || y == z;
        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = user.ToString();
                footer.IconUrl = user.GetAvatarUrl();
            })
            .WithDescription($"[ {Fruits[x]} {Fruits[y]} {Fruits[z]} ] : {(match ? "Congrats, won." : "No match, lost.")}")
            .WithColor(match ? Color.Orange : Color.Red)
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task OwoifyAsync(ITextChannel channel, IUser user, string content)
    {
        content = content.Trim();
        content = content.Replace('l', 'w');
        content = content.Replace('r', 'w');
        content = content.Replace('L', 'W');
        content = content.Replace('R', 'W');
        content = content.Replace("ove", "uv");
        content = content.Replace(".", OwoFaces[Random.Next()]);
        content = content.Replace("!", OwoFaces[Random.Next()]);
        content = content.Replace("?", OwoFaces[Random.Next()]);
        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = user.Username;
                footer.IconUrl = user.GetAvatarUrl();
            })
            .WithDescription($"🪙 {((Random.Next(2) == 0) ? "Heads" : "Tails")}")
            .WithColor(Color.Magenta)
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }
}
