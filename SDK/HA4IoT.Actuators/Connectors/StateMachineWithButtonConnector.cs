using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators.Connectors
{
    public static class StateMachineWithButtonConnector
    {
        public static IStateMachine ConnectMoveNextAndToggleOffWith(this IStateMachine stateMachineActuator, IButton button)
        {
            button.GetPressedShortlyTrigger().Attach(() => stateMachineActuator.SetNextState());

            if (stateMachineActuator.HasOffState)
            {
                button.GetPressedLongTrigger().Attach(() => stateMachineActuator.TurnOff());
            }

            return stateMachineActuator;
        }
    }
}
