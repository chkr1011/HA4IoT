namespace HA4IoT.Contracts.Actuators
{
    public class StateMachineStateChangedEventArgs : ValueChangedEventArgs<StateId>
    {
        public StateMachineStateChangedEventArgs(StateId oldState, StateId newState) 
            : base(oldState, newState)
        {
        }
    }
}
