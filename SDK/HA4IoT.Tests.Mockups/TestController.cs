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
        }

        public void SetTime(TimeSpan value)
        {
            ((TestHomeAutomationTimer)Timer).SetTime(value);
        }
    }
}
