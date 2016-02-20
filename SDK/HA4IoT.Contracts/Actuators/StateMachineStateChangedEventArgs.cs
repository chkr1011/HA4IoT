namespace HA4IoT.Contracts.Actuators
{
    public class StateMachineStateChangedEventArgs : ValueChangedEventArgs<string>
    {
        public StateMachineStateChangedEventArgs(string oldState, string newState) : base(oldState, newState)
        {
        }
    }
}
