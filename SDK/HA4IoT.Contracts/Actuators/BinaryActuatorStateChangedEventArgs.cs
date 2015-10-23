namespace HA4IoT.Contracts.Actuators
{
    public class BinaryActuatorStateChangedEventArgs : ValueChangedEventArgsBase<BinaryActuatorState>
    {
        public BinaryActuatorStateChangedEventArgs(BinaryActuatorState oldState, BinaryActuatorState newState) : base(oldState, newState)
        {
        }
    }
}
