using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButton : ISensor
    {
        IButtonSettings Settings { get; }
        ITrigger GetPressedShortlyTrigger();
        ITrigger GetPressedLongTrigger();
    }
}
