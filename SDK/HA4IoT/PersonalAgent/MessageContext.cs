using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.PersonalAgent
{
    public class MessageContext
    {
        public string Kind { get; set; } // MessageContextKind > Text / Speech

        public string Text { get; set; }

        public HashSet<string> Words { get; } = new HashSet<string>();

        public HashSet<ComponentId> IdentifiedComponentIds { get; } = new HashSet<ComponentId>();

        public HashSet<ComponentState> IdentifiedComponentStates { get; } = new HashSet<ComponentState>();

        public HashSet<ComponentId> FilteredComponentIds { get; } = new HashSet<ComponentId>();

        public HashSet<AreaId> IdentifiedAreaIds { get; } = new HashSet<AreaId>();

        public bool ContainsWord(string word)
        {
            if (word == null) throw new ArgumentNullException(nameof(word));

            return Words.Any(w => string.Equals(w, word, StringComparison.CurrentCultureIgnoreCase));
        }

        public Match GetPatternMatch(string pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            return Regex.Match(Text ?? string.Empty, pattern, RegexOptions.IgnoreCase);
        }
    }
}
