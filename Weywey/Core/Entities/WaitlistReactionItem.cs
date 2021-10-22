using Discord.WebSocket;
using System;
using System.Linq.Expressions;

namespace Weywey.Core.Entities
{
    public class WaitlistReactionItem
    {
        public WaitlistReactionItem(ulong messageId, TimeSpan duration, Expression<Func<SocketReaction, bool>> filter)
        {
            MessageId = messageId;
            Duration = duration;
            Filter = filter == null ? null : filter.Compile();
            CreatedAt = DateTime.UtcNow;
        }

        public ulong MessageId { get; set; }
        public TimeSpan Duration { get; set; }
        public Func<SocketReaction, bool> Filter { get; set; }
        public SocketReaction Reaction { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Expired
            => DateTime.UtcNow.Subtract(CreatedAt).TotalSeconds >= Duration.TotalSeconds;
    }
}
