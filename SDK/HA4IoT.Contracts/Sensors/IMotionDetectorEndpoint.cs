using System;

namespace HA4IoT.Contracts.Sensors
{
    public interface IMotionDetectorEndpoint
    {
        event EventHandler MotionDetected;

        event EventHandler DetectionCompleted;
    }
}
