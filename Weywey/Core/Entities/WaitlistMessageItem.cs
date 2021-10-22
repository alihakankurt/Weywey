using Discord.WebSocket;
using System;
using System.Linq.Expressions;

namespace Weywey.Core.Entities
{
    public class WaitlistMessageItem
    {
        public WaitlistMessageItem(ulong channelId, TimeSpan duration, Predicate<SocketMessage> filter)
        {
            ChannelId = channelId;
            Duration = duration;
            Filter = filter == null ? null : filter;
            CreatedAt = DateTime.UtcNow;
        }

        public ulong ChannelId { get; set; }
        public TimeSpan Duration { get; set; }
        public Predicate<SocketMessage> Filter { get; set; }
        public SocketMessage Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Expired
            => DateTime.UtcNow.Subtract(CreatedAt).TotalSeconds >= Duration.TotalSeconds;
    }
}
