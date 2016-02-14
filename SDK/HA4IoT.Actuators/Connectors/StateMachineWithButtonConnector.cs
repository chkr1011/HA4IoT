using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators.Connectors
{
    public static class StateMachineWithButtonConnector
    {
        public static StateMachine ConnectMoveNextAndToggleOffWith(this StateMachine stateMachineActuator, IButton button)
        {
            button.GetPressedShortlyTrigger().Attach(() => stateMachineActuator.SetNextState());

            if (stateMachineActuator.HasOffState)
            {
                button.GetPressedLongTrigger().Attach(() => stateMachineActuator.SetState(BinaryActuatorState.Off.ToString()));
            }

            return stateMachineActuator;
        }
    }
}
