using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts
{
    public class MotionDetectorStateChangedEventArgs : ValueChangedEventArgsBase<MotionDetectorState>
    {
        public MotionDetectorStateChangedEventArgs(MotionDetectorState oldValue, MotionDetectorState newValue) : base(oldValue, newValue)
        {
        }
    }
}
