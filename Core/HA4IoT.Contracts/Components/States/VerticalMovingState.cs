namespace HA4IoT.Contracts.Components.States
{
    public class VerticalMovingState : EnumBasedState<VerticalMovingStateValue>
    {
        public static readonly VerticalMovingState MovingUp = new VerticalMovingState(VerticalMovingStateValue.MovingUp);
        public static readonly VerticalMovingState MovingDown = new VerticalMovingState(VerticalMovingStateValue.MovingDown);
        public static readonly VerticalMovingState Stopped = new VerticalMovingState(VerticalMovingStateValue.Stopped);

        public VerticalMovingState(VerticalMovingStateValue value) : base(value)
        {
        }
    }
}
