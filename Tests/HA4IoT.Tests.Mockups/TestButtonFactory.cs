using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestButtonFactory
    {
        private readonly ITimerService _timerService;

        public TestButtonFactory(ITimerService timerService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));

            _timerService = timerService;
        }

        public TestButton CreateTestButton()
        {
            return new TestButton(ComponentIdFactory.EmptyId, new TestButtonEndpoint(), _timerService);
        }
    }
}