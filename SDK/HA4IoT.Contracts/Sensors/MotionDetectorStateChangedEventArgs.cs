namespace HA4IoT.Contracts.Sensors
{
    public class MotionDetectorStateChangedEventArgs : ValueChangedEventArgs<MotionDetectorState>
    {
        public MotionDetectorStateChangedEventArgs(MotionDetectorState oldValue, MotionDetectorState newValue) : base(oldValue, newValue)
        {
        }
    }
}
