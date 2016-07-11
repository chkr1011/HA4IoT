using System;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core;

namespace HA4IoT.Tests.Mockups
{
    public class TestController : ControllerBase
    {
        public TestController()
        {
            Log.Instance = new TestLogger();
            Timer = new TestHomeAutomationTimer();

            ServiceLocator.RegisterService(new TestDaylightService());
            ServiceLocator.RegisterService(new TestDateTimeService());
            ServiceLocator.RegisterService(new SystemInformationService());
        }

        public void SetTime(TimeSpan value)
        {
            ServiceLocator.GetService<TestDateTimeService>().SetTime(value);
        }
    }
}
