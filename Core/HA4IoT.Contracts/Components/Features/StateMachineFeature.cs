using System.Collections.Generic;

namespace HA4IoT.Contracts.Components.Features
{
    public class StateMachineFeature : IComponentFeature
    {
        public HashSet<string> SupportedStates { get; } = new HashSet<string>();
    }
}
