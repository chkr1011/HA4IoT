using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators
{
    public class StateMachineStateChangedEventArgs : ValueChangedEventArgsBase<string>
    {
        public StateMachineStateChangedEventArgs(string oldState, string newState) : base(oldState, newState)
        {
        }
    }
}
