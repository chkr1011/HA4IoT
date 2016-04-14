using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Sensors.MotionDetectors;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetector : MotionDetector
    {
        public TestMotionDetector(ComponentId id, TestMotionDetectorEndpoint endpoint, IHomeAutomationTimer timer) 
            : base(id, endpoint, timer)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint;
        }

        public TestMotionDetectorEndpoint Endpoint { get; }

        public void DetectMotion()
        {
            OnMotionDetected();
        }

        public void CompleteMotionDetection()
        {
            OnDetectionCompleted();
        }

        public void TriggerMotionDetection()
        {
            OnMotionDetected();
            OnDetectionCompleted();
        }
    }
}