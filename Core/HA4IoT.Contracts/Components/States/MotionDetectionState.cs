namespace HA4IoT.Contracts.Components.States
{
    public class MotionDetectionState : EnumBasedState<MotionDetectionStateValue>
    {
        public static readonly MotionDetectionState Idle = new MotionDetectionState(MotionDetectionStateValue.Idle);
        public static readonly MotionDetectionState MotionDetected = new MotionDetectionState(MotionDetectionStateValue.MotionDetected);

        public MotionDetectionState(MotionDetectionStateValue value) : base(value)
        {
        }
    }
}
