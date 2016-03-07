using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IMotionDetectorEndpoint
    {
        event EventHandler MotionDetected;

        event EventHandler DetectionCompleted;
    }
}
