using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IMotionDetector : IActuator
    {
        event EventHandler MotionDetected;
        event EventHandler DetectionCompleted;

        event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;
            
        MotionDetectorState GetState();
    }
}
