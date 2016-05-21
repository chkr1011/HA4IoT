using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Tests.Mockups
{
    public class TestButtonFactory
    {
        private readonly IHomeAutomationTimer _timer;

        public TestButtonFactory(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public TestButton CreateTestButton()
        {
            return new TestButton(ComponentIdFactory.EmptyId, new TestButtonEndpoint(), _timer);
        }
    }
}