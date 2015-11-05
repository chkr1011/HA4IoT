using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IMotionDetector : IActuatorBase
    {
        event EventHandler MotionDetected;
        event EventHandler DetectionCompleted;

        bool IsMotionDetected { get; }
    }
}
