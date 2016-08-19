using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetectorFactory
    {
        private readonly ISchedulerService _schedulerService;

        public TestMotionDetectorFactory(ISchedulerService schedulerService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            _schedulerService = schedulerService;
        }

        public TestMotionDetector CreateTestMotionDetector()
        {
            return new TestMotionDetector(ComponentIdFactory.EmptyId, new TestMotionDetectorEndpoint(), _schedulerService);
        }
    }
}