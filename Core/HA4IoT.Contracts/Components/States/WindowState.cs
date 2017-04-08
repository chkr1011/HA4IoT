namespace HA4IoT.Contracts.Components.States
{
    public class WindowState : EnumBasedState<WindowStateValue>
    {
        public static readonly WindowState Open = new WindowState(WindowStateValue.Open);
        public static readonly WindowState Closed = new WindowState(WindowStateValue.Closed);
        public static readonly WindowState TildOpen = new WindowState(WindowStateValue.TildOpen);

        public WindowState(WindowStateValue value) : base(value)
        {
        }
    }
}
