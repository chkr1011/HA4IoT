using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class StateMachineFeature : IComponentFeature
    {
        public HashSet<string> SupportedStates { get; } = new HashSet<string>();

        public JToken Serialize()
        {
            return JObject.FromObject(this);
        }
    }
}
