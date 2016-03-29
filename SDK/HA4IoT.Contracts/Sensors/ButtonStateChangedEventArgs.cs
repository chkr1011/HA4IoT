namespace HA4IoT.Contracts.Sensors
{
    public class ButtonStateChangedEventArgs : ValueChangedEventArgs<ButtonState>
    {
        public ButtonStateChangedEventArgs(ButtonState oldValue, ButtonState newValue) : base(oldValue, newValue)
        {
        }
    }
}
