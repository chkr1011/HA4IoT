using System;

namespace CK.HomeAutomation.Actuators.Contracts
{
    public interface IMotionDetector : IActuatorBase
    {
        event EventHandler MotionDetected;
        event EventHandler DetectionCompleted;

        bool IsMotionDetected { get; }
    }
}
