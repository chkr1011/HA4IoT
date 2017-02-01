using System;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetectorEndpoint : IMotionDetectorAdapter
    {
        public event EventHandler MotionDetected;
        public event EventHandler MotionDetectionCompleted;

        public void DetectMotion()
        {
            MotionDetected?.Invoke(this, EventArgs.Empty);
        }

        public void CompleteDetection()
        {
            MotionDetectionCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
