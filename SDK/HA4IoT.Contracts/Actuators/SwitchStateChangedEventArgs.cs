namespace HA4IoT.Contracts.Actuators
{
    public class SwitchStateChangedEventArgs : ValueChangedEventArgsBase<SwitchState>
    {
        public SwitchStateChangedEventArgs(SwitchState oldValue, SwitchState newValue) : base(oldValue, newValue)
        {
        }
    }
}
