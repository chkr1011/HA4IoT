using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutterFactory
    {
        private readonly IHomeAutomationTimer _timer;

        public TestRollerShutterFactory(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public TestRollerShutter CreateTestRollerShutter()
        {
            return new TestRollerShutter(ComponentIdFactory.EmptyId, new TestRollerShutterEndpoint(), _timer);
        }
    }
}