using Discord.Commands;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Weywey.Core.Services;

namespace Weywey.Core.Commands.Miscellaneous;

public partial class MiscellaneousModule : ModuleBase<SocketCommandContext>
{
    [Name("Wolfram Alpha")]
    [Command("wolfram", RunMode = RunMode.Async)]
    [Summary("Ask to Wolfram Alpha what you want.")]
    public async Task WolframAlphaCommand([Remainder][Summary("The question for asking to Wolfram Alpha")] string query)
    {
        await Context.Channel.TriggerTypingAsync();
        var client = ProviderService.GetService<HttpClient>();
        var result = await client.GetStringAsync($@"https://api.wolframalpha.com/v1/result?i={WebUtility.HtmlEncode(query)}&appid={ConfigurationService.WolframToken}");
        await ReplyAsync(result);
    }
}
