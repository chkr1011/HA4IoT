using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Tests.Mockups
{
    public class TestHomeAutomationTimer : IHomeAutomationTimer
    {
        public event EventHandler<TimerTickEventArgs> Tick;

        public TimeSpan CurrentTime { get; set; }

        public DateTime CurrentDateTime { get; set; }

        public TimedAction In(TimeSpan dueTime)
        {
            return new TimedAction(dueTime, TimeSpan.Zero, this);
        }

        public TimedAction Every(TimeSpan interval)
        {
            return new TimedAction(TimeSpan.FromMilliseconds(1), TimeSpan.Zero, this);
        }
    }
}
