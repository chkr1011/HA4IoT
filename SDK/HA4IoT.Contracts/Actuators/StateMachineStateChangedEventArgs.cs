namespace HA4IoT.Contracts.Actuators
{
    public class StateMachineStateChangedEventArgs : ValueChangedEventArgs<StateMachineStateId>
    {
        public StateMachineStateChangedEventArgs(StateMachineStateId oldState, StateMachineStateId newState) 
            : base(oldState, newState)
        {
        }
    }
}
