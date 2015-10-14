using System;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Contracts;

namespace CK.HomeAutomation.Tests.Mockups
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
