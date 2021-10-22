using Discord.Commands;
using System;
using System.Linq;
using Weywey.Core.Services;

namespace Weywey.Core.Extensions
{
    public static class CommandExtensions
    {
        public static string GetSyntax(this CommandInfo source)
           => $"```cs\n\"{ConfigurationService.Prefix}{string.Join(" | ", source.Aliases)} {string.Join(' ', source.Parameters.Select(p => p.IsOptional ? $"[{p.Name}]" : $"<{p.Name}>"))}\"```";
    }
}
