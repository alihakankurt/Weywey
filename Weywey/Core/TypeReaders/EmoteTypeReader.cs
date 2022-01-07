using Discord.Commands;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Weywey.Core.TypeReaders;

public class EmoteTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        ulong emoteId;

        if (!ulong.TryParse(input, out emoteId))
        {
            var match = Regex.Match(input, "^<a?:.+:([0-9]{18})>$");

            if (!match.Success)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid emote."));

            Console.WriteLine(match.Groups.Values.First().Value);
            emoteId = Convert.ToUInt64(match.Groups.Values.ElementAt(1).Value);
        }

        var emote = context.Guild.GetEmoteAsync(emoteId).Result;

        if (emote is null)
            return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "No emote existing with this id."));

        return Task.FromResult(TypeReaderResult.FromSuccess(emote));
    }
}
