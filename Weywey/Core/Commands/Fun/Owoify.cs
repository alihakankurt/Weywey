using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Weywey.Core.Commands.Fun
{
    public partial class FunModule : ModuleBase<SocketCommandContext>
    {
        [Name("OwOify")]
        [Command("owoify", RunMode = RunMode.Async)]
        [Summary("Owoify your message.")]
        public async Task OwoifyCommand([Remainder] string message)
        {
            message = Regex.Replace(message, "(?:r|l)", "w");
            message = Regex.Replace(message, "(?:R|L)", "W");
            message = Regex.Replace(message, "ove", "uv");
            message = Regex.Replace(message, "(?<!\\@)\\!+", " " + _owoFaces[new Random().Next(_owoFaces.Length)] + " ");
            await ReplyAsync(message);
        }

        private static readonly string[] _owoFaces = { "(・`ω´・)", ";;w;;", "owo", "UwU", ">w<", "^w^" };
    }
}
