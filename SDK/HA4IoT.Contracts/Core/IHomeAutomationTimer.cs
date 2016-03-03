using System;

namespace HA4IoT.Contracts.Core
{
    public interface IHomeAutomationTimer
    {
        event EventHandler<TimerTickEventArgs> Tick;

        TimeSpan CurrentTime { get; }

        DateTime CurrentDateTime { get; }

        TimedAction In(TimeSpan dueTime);

        TimedAction Every(TimeSpan interval);
    }
}
