using System;

namespace HA4IoT.Contracts.Core
{
    public interface IHomeAutomationTimer
    {
        event EventHandler<TimerTickEventArgs> Tick;

        // TODO: Migrate all code to use DateTimeService instead CurrentTime and CurrentDateTime here!

        TimeSpan CurrentTime { get; }

        DateTime CurrentDateTime { get; }

        TimedAction In(TimeSpan dueTime);

        TimedAction Every(TimeSpan interval);
    }
}
