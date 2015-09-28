using System;

namespace CK.HomeAutomation.Actuators
{
    public class StateMachineStateChangedEventArgs : EventArgs
    {
        public StateMachineStateChangedEventArgs(string oldState, string newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public string OldState { get; }

        public string NewState { get; }
    }
}
