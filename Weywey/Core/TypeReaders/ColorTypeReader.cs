using Discord;
using Discord.Commands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Weywey.Core.TypeReaders
{
    public class ColorTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Regex.IsMatch(input, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
                return Task.FromResult(TypeReaderResult.FromSuccess(new Color(uint.Parse(input.Trim('#'), System.Globalization.NumberStyles.HexNumber))));

            else if (int.TryParse(input, out int result))
                return Task.FromResult(TypeReaderResult.FromSuccess(new Color((uint)result)));

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Can't convert {input} to color."));
        }
    }
}
