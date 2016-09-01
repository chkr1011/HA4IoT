using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutterFactory
    {
        private readonly ITimerService _timerService;
        private readonly ISchedulerService _schedulerService;
        private readonly ISettingsService _settingsService;

        public TestRollerShutterFactory(ITimerService timerService, ISchedulerService schedulerService, ISettingsService settingsService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            _timerService = timerService;
            _schedulerService = schedulerService;
            _settingsService = settingsService;
        }

        public TestRollerShutter CreateTestRollerShutter()
        {
            return new TestRollerShutter(
                ComponentIdFactory.EmptyId,
                new TestRollerShutterEndpoint(), 
                _timerService, 
                _schedulerService,
                _settingsService);
        }
    }
}