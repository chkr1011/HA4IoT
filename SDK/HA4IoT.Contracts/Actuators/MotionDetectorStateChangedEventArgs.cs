namespace HA4IoT.Contracts.Actuators
{
    public class MotionDetectorStateChangedEventArgs : ValueChangedEventArgsBase<MotionDetectorState>
    {
        public MotionDetectorStateChangedEventArgs(MotionDetectorState oldValue, MotionDetectorState newValue) : base(oldValue, newValue)
        {
        }
    }
}
