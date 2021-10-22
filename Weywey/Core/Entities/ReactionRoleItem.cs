using System;
using System.Collections.Generic;
using System.Text;

namespace Weywey.Core.Entities
{
    public class ReactionRoleItem
    {
        public ulong MessageId { get; set; }
        public Dictionary<string, ulong> Roles { get; set; }
    }
}
