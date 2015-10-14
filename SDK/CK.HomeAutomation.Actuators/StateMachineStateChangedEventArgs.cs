namespace CK.HomeAutomation.Actuators
{
    public class StateMachineStateChangedEventArgs : ValueChangedEventArgsBase<string>
    {
        public StateMachineStateChangedEventArgs(string oldState, string newState) : base(oldState, newState)
        {
        }
    }
}
