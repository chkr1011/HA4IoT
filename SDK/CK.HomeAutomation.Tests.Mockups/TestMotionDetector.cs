using System;
using CK.HomeAutomation.Actuators.Contracts;

namespace CK.HomeAutomation.Tests.Mockups
{
    public class TestMotionDetector : IMotionDetector
    {
        public event EventHandler MotionDetected;
        public event EventHandler DetectionCompleted;

        public bool IsMotionDetected { get; set; }

        public string Id { get; set; }

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
