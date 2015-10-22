using System;

namespace HA4IoT.Actuators.Contracts
{
    public interface IMotionDetector : IActuatorBase
    {
        event EventHandler MotionDetected;
        event EventHandler DetectionCompleted;

        bool IsMotionDetected { get; }
    }
}
