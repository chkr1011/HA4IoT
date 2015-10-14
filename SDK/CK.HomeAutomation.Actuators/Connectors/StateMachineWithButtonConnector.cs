
using CK.HomeAutomation.Actuators.Contracts;

namespace CK.HomeAutomation.Actuators.Connectors
{
    public static class StateMachineWithButtonConnector
    {
        public static StateMachine ConnectMoveNextAndToggleOffWith(this StateMachine stateMachineActuator, IButton button)
        {
            button.PressedShort += (s, e) => stateMachineActuator.ApplyNextState();

            if (stateMachineActuator.HasOffState)
            {
                button.PressedLong += (s, e) => stateMachineActuator.ApplyState(BinaryActuatorState.Off.ToString());
            }

            return stateMachineActuator;
        }
    }
}
