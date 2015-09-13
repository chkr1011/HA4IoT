using System;

namespace CK.HomeAutomation.Actuators
{
    public class BinaryActuatorStateChangedEventArgs : EventArgs
    {
        public BinaryActuatorStateChangedEventArgs(BinaryActuatorState oldState, BinaryActuatorState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public BinaryActuatorState OldState { get; }
        public BinaryActuatorState NewState { get; }
    }
}
