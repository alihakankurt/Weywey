using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Miscellaneous;

public static class MiscellaneousCommands
{
    private static readonly DiscordSocketClient Client;
    private static readonly HttpClient HttpClient;
    private static readonly Random Random;
    private static readonly Dictionary<ulong, int> BinaryQuizs;

    static MiscellaneousCommands()
    {
        Client = ProviderService.GetService<DiscordSocketClient>();
        HttpClient = ProviderService.GetService<HttpClient>();
        Random = new();
        BinaryQuizs = new();

        Client.ButtonExecuted += Client_ButtonExecuted;
    }

    public static async Task TextToBinaryAsync(ITextChannel channel, IUser user, string text)
    {
        var bytes = Encoding.ASCII.GetBytes(text);
        var binary = string.Join(" ", bytes.Select(x => ToBinary(x, 8)));
        var embed = new EmbedBuilder()
            .AddField("Text", text, false)
            .AddField("Binary", binary, false)
            .WithColor(220, 20, 60)
            .WithCurrentTimestamp()
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task DecimalToBinaryAsync(ITextChannel channel, IUser user, int num)
    {
        var embed = new EmbedBuilder()
            .AddField("Decimal", num, false)
            .AddField("Binary", ToBinary(num, 32), false)
            .WithColor(220, 20, 60)
            .WithCurrentTimestamp()
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    public static async Task BinaryQuizAsync(ITextChannel channel, IUser user)
    {
        if (BinaryQuizs.ContainsKey(user.Id))
            return;

        int[] values = { Random.Next(256), Random.Next(256), Random.Next(256) };
        int index = Random.Next(3);

        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = $"Invoked by {user.Username}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .AddField("Value", values[index], false)
            .WithDescription($"Choose correct answer in 10 seconds.")
            .WithColor(Color.Purple)
            .Build();

        var componentBuilder = new ComponentBuilder();
        for (int i = 0; i < values.Length; i++)
        {
            string label = ToBinary(values[i], 8);
            componentBuilder.WithButton(label, $"binary-{user.Id}-{label}", ButtonStyle.Primary);
        }

        var message = await channel.SendMessageAsync(embed: embed, components: componentBuilder.Build());

        BinaryQuizs[user.Id] = values[index];
        await Task.Delay(10000);

        if (BinaryQuizs.ContainsKey(user.Id))
        {
            BinaryQuizs.Remove(user.Id);
            await message.DeleteAsync();
            await channel.SendMessageAsync($"Answer was `{ToBinary(values[index], 8)}`");
        }
    }

    public static async Task WolframAlphaAsync(ITextChannel channel, IUser user, string query)
    {
        await channel.TriggerTypingAsync();
        var result = await HttpClient.GetStringAsync($@"https://api.wolframalpha.com/v1/result?i={WebUtility.HtmlEncode(query)}&appid={ConfigurationService.WolframToken}");
        await channel.SendMessageAsync(result);
    }

    public static async Task SendColorAsync(ITextChannel channel, IUser user, Color color)
    {
        var embed = new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = $"Requested by {user.Username}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .WithDescription($"Hex: {color}\nR: {color.R}\nG: {color.G}\nB: {color.B}")
            .WithColor(color)
            .WithImageUrl("https://some-random-api.ml/canvas/colorviewer?hex=" + color.ToString().Trim('#'))
            .Build();
        await channel.SendMessageAsync(embed: embed);
    }

    private static string ToBinary(int value, int length)
    {
        if (length > 0)
            return ToBinary(value >> 1, length - 1) + (value & 1);

        return "";
    }

    private static async Task Client_ButtonExecuted(SocketMessageComponent component)
    {
        if (component.Data.CustomId.StartsWith("binary") && (component.User.Id.ToString() == component.Data.CustomId[7..25]))
        {
            string binary = ToBinary(BinaryQuizs[component.User.Id], 8);
            await component.Channel.SendMessageAsync(component.Data.CustomId.EndsWith(binary) ? "Correct." : $"Answer was `{binary}`");
            await component.Message.DeleteAsync();
            BinaryQuizs.Remove(component.User.Id);
        }
        await component.DeferAsync();
    }
}
