using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Weywey.Core.Extensions
{
    public static class Extensions
    {
        public static string ClearCodeBlock(this string source)
            => (source.StartsWith("```") && source.EndsWith("```")) ? string.Join("\n", source.Trim('`').Split("\n").Where((l, i) => i != 0)) : source;

        public static string WithCodeBlock(this string source, string language = "cs")
            => $"```{language}\n{source}```";

        public static string ToBinary(this string source)
            => string.Join("", Encoding.ASCII.GetBytes(source).Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));

        public static string FromBinary(this string source)
        {
            var list = new List<byte>();

            for (int i = 0; i < source.Length; i += 8)
                list.Add(Convert.ToByte(source.Substring(i, 8), 2));

            return Encoding.ASCII.GetString(list.ToArray());
        }

        public static string SeperateFromCaps(this string source)
            => new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace).Replace(source, " ");

        public static short ToInt16(this string source)
        {
            if (short.TryParse(source, out short result))
                return result;

            return 0;
        }

        public static int ToInt32(this string source)
        {
            if (int.TryParse(source, out int result))
                return result;

            return 0;
        }

        public static long ToInt64(this string source)
        {
            if (long.TryParse(source, out long result))
                return result;

            return 0;
        }

        public static ushort ToUInt16(this string source)
        {
            if (ushort.TryParse(source, out ushort result))
                return result;

            return 0;
        }

        public static uint ToUInt32(this string source)
        {
            if (uint.TryParse(source, out uint result))
                return result;

            return 0;
        }

        public static ulong ToUInt64(this string source)
        {
            if (ulong.TryParse(source, out ulong result))
                return result;

            return 0;
        }
    }
}
