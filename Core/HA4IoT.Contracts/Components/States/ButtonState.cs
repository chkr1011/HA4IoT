namespace HA4IoT.Contracts.Components.States
{
    public class ButtonState : EnumBasedState<ButtonStateValue>
    {
        public static readonly ButtonState Released = new ButtonState(ButtonStateValue.Released);
        public static readonly ButtonState Pressed = new ButtonState(ButtonStateValue.Pressed);

        public ButtonState(ButtonStateValue value) : base(value)
        {
        }
    }
}
