using System;

namespace HA4IoT.Contracts.Core
{
    public interface IHomeAutomationTimer
    {
        event EventHandler<TimerTickEventArgs> Tick;
    }
}
