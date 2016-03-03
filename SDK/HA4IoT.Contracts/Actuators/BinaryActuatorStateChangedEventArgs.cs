namespace HA4IoT.Contracts.Actuators
{
    public class BinaryActuatorStateChangedEventArgs : ValueChangedEventArgs<BinaryActuatorState>
    {
        public BinaryActuatorStateChangedEventArgs(BinaryActuatorState oldState, BinaryActuatorState newState) : base(oldState, newState)
        {
        }
    }
}
