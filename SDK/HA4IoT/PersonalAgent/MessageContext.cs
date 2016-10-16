using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.PersonalAgent
{
    public class MessageContext
    {
        public MessageContext(IInboundMessage originalMessage)
        {
            if (originalMessage == null) throw new ArgumentNullException(nameof(originalMessage));

            OriginalMessage = originalMessage;

            Words = new HashSet<string>();

            IdentifiedAreaIds = new HashSet<AreaId>();
            IdentifiedComponentIds = new HashSet<ComponentId>();
            FilteredComponentIds = new HashSet<ComponentId>();
            IdentifiedComponentStates = new HashSet<ComponentState>();
        }

        public IInboundMessage OriginalMessage { get; }

        public HashSet<string> Words { get; }

        public HashSet<ComponentId> IdentifiedComponentIds { get; } 

        public HashSet<ComponentState> IdentifiedComponentStates { get; }

        public HashSet<ComponentId> FilteredComponentIds { get; } 

        public HashSet<AreaId> IdentifiedAreaIds { get; }

        public bool GetContainsWord(string word)
        {
            if (word == null) throw new ArgumentNullException(nameof(word));

            return Words.Any(w => string.Equals(w, word, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool GetContainsPattern(string pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            return GetPatternMatch(pattern).Success;
        }

        public Match GetPatternMatch(string pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            return Regex.Match(OriginalMessage.Text, pattern, RegexOptions.IgnoreCase);
        }
    }
}
