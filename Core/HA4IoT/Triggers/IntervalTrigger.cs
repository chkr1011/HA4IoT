using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Triggers
{
    public class IntervalTrigger : Trigger
    {
        public IntervalTrigger(TimeSpan interval, ISchedulerService scheduleService)
        {
            if (scheduleService == null) throw new ArgumentNullException(nameof(scheduleService));

            scheduleService.Register("IntervalTrigger-" + Guid.NewGuid(), interval, () => Execute());
        }
    }
}
