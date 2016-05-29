using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetectorFactory
    {
        private readonly IHomeAutomationTimer _timer;

        public TestMotionDetectorFactory(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public TestMotionDetector CreateTestMotionDetector()
        {
            return new TestMotionDetector(ComponentIdFactory.EmptyId, new TestMotionDetectorEndpoint(), _timer);
        }
    }
}