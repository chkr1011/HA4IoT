using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface ISwitch : IActuator
    {
        ITrigger GetTurnedOnTrigger();
        ITrigger GetTurnedOffTrigger();
    }
}
