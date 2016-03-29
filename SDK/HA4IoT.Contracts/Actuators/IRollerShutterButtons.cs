using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutterButtons : IActuator
    {
        IButton Up { get; }
        IButton Down { get; }
    }
}
