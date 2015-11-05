using System;

namespace HA4IoT.Core.Timer
{
    public interface IHomeAutomationTimer
    {
        event EventHandler<TimerTickEventArgs> Tick;

        TimeSpan CurrentTime { get; }

        TimedAction In(TimeSpan dueTime);

        TimedAction Every(TimeSpan interval);

        void Run();
    }
}
