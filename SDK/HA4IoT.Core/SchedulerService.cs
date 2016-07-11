using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Core
{
    public class SchedulerService : ServiceBase, ISchedulerService
    {
        private readonly IHomeAutomationTimer _timer;

        public SchedulerService(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public TimedAction In(TimeSpan dueTime)
        {
            return new TimedAction(dueTime, TimeSpan.Zero, _timer);
        }

        public TimedAction Every(TimeSpan interval)
        {
            return new TimedAction(TimeSpan.FromMilliseconds(1), interval, _timer);
        }
    }
}
