using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Commands;

namespace HA4IoT.PersonalAgent
{
    public class MessageContext
    {
        public MessageContextKind Kind { get; set; }

        public string Intent { get; set; } // TODO: To Enum

        public string Text { get; set; }

        public Dictionary<string, string> Slots { get; } = new Dictionary<string, string>();

        public HashSet<string> IdentifiedComponentIds { get; } = new HashSet<string>();

        public HashSet<ICommand> IdentifiedCommands { get; } = new HashSet<ICommand>();

        public HashSet<string> AffectedComponentIds { get; } = new HashSet<string>();

        public HashSet<string> IdentifiedAreaIds { get; } = new HashSet<string>();

        public string Answer { get; set; }

        public Match GetPatternMatch(string pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            return Regex.Match(Text ?? string.Empty, pattern, RegexOptions.IgnoreCase);
        }
    }
}
