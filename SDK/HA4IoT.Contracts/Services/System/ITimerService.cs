using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Services.System
{
    public interface ITimerService
    {
        event EventHandler<TimerTickEventArgs> Tick;

        void Run();
    }
}
