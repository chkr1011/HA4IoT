using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetector : IMotionDetector
    {
        public event EventHandler<ActuatorIsEnabledChangedEventArgs> IsEnabledChanged;
        public event EventHandler MotionDetected;
        public event EventHandler DetectionCompleted;

        public string Id { get; set; }
        public bool IsEnabled { get; }

        public bool IsMotionDetected { get; set; }

        public void WalkIntoMotionDetector()
        {
            MotionDetected?.Invoke(this, EventArgs.Empty);
        }

        public void FireDetectionCompleted()
        {
            DetectionCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
