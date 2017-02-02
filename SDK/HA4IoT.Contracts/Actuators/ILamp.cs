using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Actuators
{
    public interface ILamp : IActuator
    {
        IAction TogglePowerStateAction { get; }
    }
}
