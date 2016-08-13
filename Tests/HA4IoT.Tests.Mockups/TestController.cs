using System;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Core;
using HA4IoT.Core.Scheduling;

namespace HA4IoT.Tests.Mockups
{
    public class TestController : ControllerBase
    {
        private readonly TestDateTimeService _dateTimeService = new TestDateTimeService();

        public TestController() 
            : base(22)
        {
            Log.Instance = new TestLogger();
            Timer = new TestHomeAutomationTimer();

            ServiceLocator.RegisterService(typeof(IDaylightService), new TestDaylightService());
            ServiceLocator.RegisterService(typeof(IDateTimeService), _dateTimeService);
            ServiceLocator.RegisterService(typeof(ISystemInformationService), new SystemInformationService());
            ServiceLocator.RegisterService(typeof(ISchedulerService), new SchedulerService(Timer));
        }

        public void SetTime(TimeSpan value)
        {
            _dateTimeService.SetTime(value);
        }
    }
}
