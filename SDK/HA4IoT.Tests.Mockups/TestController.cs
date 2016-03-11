using System;
using HA4IoT.Core;

namespace HA4IoT.Tests.Mockups
{
    public class TestController : ControllerBase
    {
        public TestController()
        {
            Logger = new TestLogger();
            Timer = new TestHomeAutomationTimer();
        }

        public void SetTime(TimeSpan value)
        {
            ((TestHomeAutomationTimer)Timer).SetTime(value);
        }
    }
}
