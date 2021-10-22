using System;

namespace Weywey.Core.Entities
{
    public class PollItem
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public string Question { get; set; }
        public string[] Options { get; set; }
        public DateTime End { get; set; }
        public bool Completed { get; set; }
    }
}
