namespace HA4IoT.Contracts.Actuators
{
    public class ButtonStateChangedEventArgs : ValueChangedEventArgsBase<ButtonState>
    {
        public ButtonStateChangedEventArgs(ButtonState oldValue, ButtonState newValue) : base(oldValue, newValue)
        {
        }
    }
}
