using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutter : IActuator
    {
        bool IsClosed { get; }

        IAction GetTurnOffAction();

        IAction GetStartMoveUpAction();

        IAction GetStartMoveDownAction();
    }
}
