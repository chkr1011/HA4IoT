using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButton : IComponent
    {
        ITrigger PressedShortlyTrigger { get; }
        ITrigger PressedLongTrigger { get; }
    }
}
