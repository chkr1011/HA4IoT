using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Actuators
{
    public interface ISocket : IActuator
    {
        IAction TogglePowerStateAction { get; }
    }
}
