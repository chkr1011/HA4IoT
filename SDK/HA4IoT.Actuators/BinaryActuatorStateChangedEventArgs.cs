namespace HA4IoT.Actuators
{
    public class BinaryActuatorStateChangedEventArgs : ValueChangedEventArgsBase<BinaryActuatorState>
    {
        public BinaryActuatorStateChangedEventArgs(BinaryActuatorState oldState, BinaryActuatorState newState) : base(oldState, newState)
        {
        }
    }
}
