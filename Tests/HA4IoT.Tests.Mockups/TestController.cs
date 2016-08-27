using System;
using HA4IoT.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Notifications;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Components;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestController : IController
    {
        private readonly TestDateTimeService _dateTimeService = new TestDateTimeService();

        public TestController()
        {
            Log.Instance = new TestLogger();

            var systemInformationService = new SystemInformationService();
            var apiService = new ApiService();

            SchedulerService = new SchedulerService(TimerService);
            var systemEventsService = new SystemEventsService(this);
            AutomationService = new AutomationService(systemEventsService, systemInformationService, apiService);
            ComponentService = new ComponentService(systemEventsService, systemInformationService, apiService);
            AreaService = new AreaService(ComponentService, AutomationService, systemEventsService, systemInformationService, apiService);
            NotificationService = new NotificationService(DateTimeService, new ApiService(), SchedulerService, systemEventsService);
        }

        public ITimerService TimerService { get; } = new TestTimerService();
        public ISchedulerService SchedulerService { get; }
        public IDateTimeService DateTimeService => _dateTimeService;
        public IDaylightService DaylightService { get; } = new TestDaylightService();
        public IComponentService ComponentService { get; }
        public IAutomationService AutomationService { get; }
        public INotificationService NotificationService { get; }
        public IAreaService AreaService { get; }

        public event EventHandler StartupCompleted;
        public event EventHandler StartupFailed;
        public event EventHandler Shutdown;

        public void SetTime(TimeSpan value)
        {
            _dateTimeService.SetTime(value);
        }
    }
}
