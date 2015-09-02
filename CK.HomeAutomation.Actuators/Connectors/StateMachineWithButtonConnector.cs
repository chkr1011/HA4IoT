
namespace CK.HomeAutomation.Actuators.Connectors
{
    public static class StateMachineWithButtonConnector
    {
        public static StateMachine ConnectMoveNextWith(this StateMachine stateMachineActuator, Button button)
        {
            button.WithShortAction(() => stateMachineActuator.ApplyNextState());

            if (stateMachineActuator.HasOffState)
            {
                button.WithLongAction(() => stateMachineActuator.ApplyState(BinaryActuatorState.Off.ToString()));
            }

            return stateMachineActuator;
        }
    }
}
