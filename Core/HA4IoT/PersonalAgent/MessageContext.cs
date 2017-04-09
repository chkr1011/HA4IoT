using System.Collections.Generic;
using HA4IoT.Contracts.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HA4IoT.PersonalAgent
{
    public class MessageContext
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageContextKind Kind { get; set; }

        // IntentContext ...
        public string Intent { get; set; } // TODO: To Enum -> ChangePowerStateIntent, ChangeDirectionStateIntent, GetTemperatureIntent, GetHumidityIntent, GetWindowStatusIntent

        public Dictionary<string, string> Slots { get; } = new Dictionary<string, string>();

        // ...IntentContext

        // TextContext
        public string Text { get; set; }

        public string Reply { get; set; }

        // ...TextContent

        public HashSet<string> IdentifiedAreaIds { get; } = new HashSet<string>();

        public HashSet<string> IdentifiedComponentIds { get; } = new HashSet<string>();

        public HashSet<ICommand> IdentifiedCommands { get; } = new HashSet<ICommand>();

        public HashSet<string> AffectedComponentIds { get; } = new HashSet<string>();
    }
}
