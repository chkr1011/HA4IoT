using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButton : ISensor
    {
        ITrigger GetPressedShortlyTrigger();
        ITrigger GetPressedLongTrigger();
    }
}
