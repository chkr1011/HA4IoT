using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts
{
    public class ButtonStateChangedEventArgs : ValueChangedEventArgsBase<ButtonState>
    {
        public ButtonStateChangedEventArgs(ButtonState oldValue, ButtonState newValue) : base(oldValue, newValue)
        {
        }
    }
}
