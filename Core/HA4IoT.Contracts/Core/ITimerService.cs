using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface ITimerService : IService
    {
        event EventHandler<TimerTickEventArgs> Tick;
    }
}
