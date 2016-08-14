using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Core;
using HA4IoT.Core.Scheduling;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutterFactory
    {
        private readonly ITimerService _timerService;

        public TestRollerShutterFactory(ITimerService timerService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));

            _timerService = timerService;
        }

        public TestRollerShutter CreateTestRollerShutter()
        {
            return new TestRollerShutter(ComponentIdFactory.EmptyId, new TestRollerShutterEndpoint(), _timerService, new SchedulerService(_timerService));
        }
    }
}