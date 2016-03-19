namespace HA4IoT.Contracts.Hardware
{
    public class BinaryStateChangedEventArgs : ValueChangedEventArgs<BinaryState>
    {
        public BinaryStateChangedEventArgs(BinaryState oldState, BinaryState newState)  : base(oldState, newState)
        {
        }
    }
}
