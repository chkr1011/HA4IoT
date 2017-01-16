using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.PersonalAgent
{
    public class MessageContext
    {
        public MessageContextKind Kind { get; set; }

        public string Intent { get; set; } // TODO: To Enum

        public string Text { get; set; }

        public Dictionary<string, string> Slots { get; } = new Dictionary<string, string>();

        public HashSet<ComponentId> IdentifiedComponentIds { get; } = new HashSet<ComponentId>();

        public HashSet<ComponentState> IdentifiedComponentStates { get; } = new HashSet<ComponentState>();

        public HashSet<ComponentId> AffectedComponentIds { get; } = new HashSet<ComponentId>();

        public HashSet<AreaId> IdentifiedAreaIds { get; } = new HashSet<AreaId>();

        public string Answer { get; set; }

        public Match GetPatternMatch(string pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            return Regex.Match(Text ?? string.Empty, pattern, RegexOptions.IgnoreCase);
        }
    }
}
