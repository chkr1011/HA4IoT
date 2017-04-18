using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Services.System
{
    public interface ITimerService : IService
    {
        event EventHandler<TimerTickEventArgs> Tick;
    }
}
