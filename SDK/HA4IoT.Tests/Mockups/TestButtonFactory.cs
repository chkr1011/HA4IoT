using System;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestButtonFactory
    {
        private readonly ITimerService _timerService;
        private readonly ISettingsService _settingsService;

        public TestButtonFactory(ITimerService timerService, ISettingsService settingsService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));

            _timerService = timerService;
            _settingsService = settingsService;
        }

        public TestButton CreateTestButton()
        {
            return new TestButton(ComponentIdGenerator.EmptyId, new TestButtonEndpoint(), _timerService, _settingsService);
        }
    }
}