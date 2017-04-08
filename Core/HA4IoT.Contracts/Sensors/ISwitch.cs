using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface ISwitch : IComponent
    {
        ITrigger TurnedOnTrigger { get; }
        ITrigger TurnedOffTrigger { get; }
    }
}
