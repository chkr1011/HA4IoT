using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButton : IComponent
    {
        ITrigger PressedShortTrigger { get; }
        ITrigger PressedLongTrigger { get; }
    }
}
