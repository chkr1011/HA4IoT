using HA4IoT.Contracts.Actions;

namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutter : IStateMachine
    {
        bool IsClosed { get; }

        IHomeAutomationAction GetTurnOffAction();

        IHomeAutomationAction GetStartMoveUpAction();

        IHomeAutomationAction GetStartMoveDownAction();
    }
}
