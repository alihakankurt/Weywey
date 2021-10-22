using Discord.WebSocket;
using System;
using System.Linq.Expressions;

namespace Weywey.Core.Entities
{
    public class WaitlistMessageItem
    {
        public WaitlistMessageItem(ulong channelId, TimeSpan duration, Expression<Func<SocketMessage, bool>> filter)
        {
            ChannelId = channelId;
            Duration = duration;
            Filter = filter == null ? null : filter.Compile();
            CreatedAt = DateTime.UtcNow;
        }

        public ulong ChannelId { get; set; }
        public TimeSpan Duration { get; set; }
        public Func<SocketMessage, bool> Filter { get; set; }
        public SocketMessage Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Expired
            => DateTime.UtcNow.Subtract(CreatedAt).TotalSeconds >= Duration.TotalSeconds;
    }
}
