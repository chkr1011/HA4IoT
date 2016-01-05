using System;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Core;
using HA4IoT.Core.Timer;

namespace HA4IoT.Tests.Mockups
{
    public class TestHomeAutomationTimer : IHomeAutomationTimer
    {
        public event EventHandler<TimerTickEventArgs> Tick;

        public TimeSpan CurrentTime { get; set; }

        public TimedAction In(TimeSpan dueTime)
        {
            return new TimedAction(dueTime, TimeSpan.Zero, this);
        }

        public TimedAction Every(TimeSpan interval)
        {
            return new TimedAction(TimeSpan.FromMilliseconds(1), TimeSpan.Zero, this);
        }

        public void Run()
        {
        }
    }
}
