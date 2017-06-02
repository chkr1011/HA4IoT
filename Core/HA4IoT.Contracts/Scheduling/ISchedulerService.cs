using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Scheduling
{
    public interface ISchedulerService : IService
    {
        void Register(string name, TimeSpan interval, Action action);

        void Register(string name, TimeSpan interval, Func<Task> action);

        void Remove(string name);
    }
}
