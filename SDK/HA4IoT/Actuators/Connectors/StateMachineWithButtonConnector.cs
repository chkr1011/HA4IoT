using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Actuators.Connectors
{
    public static class StateMachineWithButtonConnector
    {
        public static IStateMachine ConnectMoveNextAndToggleOffWith(this IStateMachine stateMachineActuator, IButton button)
        {
            button.GetPressedShortlyTrigger().Attach(() => stateMachineActuator.SetNextState());

            if (stateMachineActuator.GetSupportsOffState())
            {
                button.GetPressedLongTrigger().Attach(() => stateMachineActuator.SetState(BinaryStateId.Off));
            }

            return stateMachineActuator;
        }
    }
}
