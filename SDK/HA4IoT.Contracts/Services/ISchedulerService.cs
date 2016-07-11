using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Services
{
    public interface ISchedulerService : IService
    {
        TimedAction In(TimeSpan dueTime);

        TimedAction Every(TimeSpan interval);
    }
}
