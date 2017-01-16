using System;

namespace HA4IoT.Contracts.Sensors
{
    public interface IMotionDetectorAdapter
    {
        event EventHandler MotionDetected;

        event EventHandler DetectionCompleted;
    }
}
