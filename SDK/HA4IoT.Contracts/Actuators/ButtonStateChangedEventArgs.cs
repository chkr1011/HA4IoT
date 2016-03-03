namespace HA4IoT.Contracts.Actuators
{
    public class ButtonStateChangedEventArgs : ValueChangedEventArgs<ButtonState>
    {
        public ButtonStateChangedEventArgs(ButtonState oldValue, ButtonState newValue) : base(oldValue, newValue)
        {
        }
    }
}
