using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestTimerService : ITimerService
    {
        public event EventHandler<TimerTickEventArgs> Tick;

        public void ExecuteTick(TimeSpan elapsedTime)
        {
            Tick?.Invoke(this, new TimerTickEventArgs { ElapsedTime = elapsedTime });
        }

        public void Startup()
        {
        }
    }
}
