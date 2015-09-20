using System;

namespace CK.HomeAutomation.Actuators.Contracts
{
    public interface IMotionDetector
    {
        event EventHandler MotionDetected;
        event EventHandler DetectionCompleted;

        string Id { get; }

        bool IsMotionDetected { get; }
    }
}
