using System;
using System.Threading.Tasks;

namespace HA4IoT.Contracts.Services.System
{
    public interface ISchedulerService : IService
    {
        IDelayedAction In(TimeSpan delay, Action action);

        void Register(string name, TimeSpan interval, Action action);

        void Register(string name, TimeSpan interval, Func<Task> action);

        void Remove(string name);
    }
}
