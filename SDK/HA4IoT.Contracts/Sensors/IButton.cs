using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButton : IStateValueSensor
    {
        ITrigger GetPressedShortlyTrigger();
        ITrigger GetPressedLongTrigger();
    }
}
