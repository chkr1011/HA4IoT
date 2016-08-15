using System;
using HA4IoT.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Components;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestController
    {
        private readonly TestDateTimeService _dateTimeService = new TestDateTimeService();

        public TestController()
        {
            Log.Instance = new TestLogger();

            var systemInformationService = new SystemInformationService();
            var systemEventsService = new SystemEventsService();
            var apiService = new ApiService();

            SchedulerService = new SchedulerService(TimerService);
            AutomationService = new AutomationService(systemEventsService, systemInformationService, apiService);
            ComponentService = new ComponentService(systemEventsService, systemInformationService, apiService);
            AreaService = new AreaService(ComponentService, AutomationService, systemEventsService, systemInformationService, apiService);
        }

        public ITimerService TimerService { get; } = new TestTimerService();
        public ISchedulerService SchedulerService { get; }
        public IDateTimeService DateTimeService => _dateTimeService;
        public IDaylightService DaylightService { get; } = new TestDaylightService();
        public IComponentService ComponentService { get; }
        public IAutomationService AutomationService { get; }
        public IAreaService AreaService { get; }

        public void SetTime(TimeSpan value)
        {
            _dateTimeService.SetTime(value);
        }
    }
}
