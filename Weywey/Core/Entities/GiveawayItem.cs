using System;

namespace Weywey.Core.Entities;

public class GiveawayItem
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }
    public ulong UserId { get; set; }
    public string Prize { get; set; }
    public DateTime End { get; set; }
    public bool Completed { get; set; }
}