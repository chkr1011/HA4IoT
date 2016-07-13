using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Tests.Mockups
{
    public class TestHomeAutomationTimer : IHomeAutomationTimer
    {
        public event EventHandler<TimerTickEventArgs> Tick;
        
        public void ExecuteTick(TimeSpan elapsedTime)
        {
            Tick?.Invoke(this, new TimerTickEventArgs(elapsedTime));
        }
    }
}
