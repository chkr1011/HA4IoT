using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButton : ISensor
    {
        IButtonSettings Settings { get; }
        ITrigger PressedShortlyTrigger { get; }
        ITrigger PressedLongTrigger { get; }
    }
}
