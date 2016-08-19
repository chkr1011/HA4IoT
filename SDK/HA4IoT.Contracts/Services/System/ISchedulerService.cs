using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Services.System
{
    public interface ISchedulerService : IService
    {
        TimedAction In(TimeSpan dueTime);

        void RegisterSchedule(string name, TimeSpan interval, Action callback);
    }
}
