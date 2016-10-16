using System;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Actuators.Triggers
{
    public class IntervalTrigger : Trigger
    {
        public IntervalTrigger(TimeSpan interval, ISchedulerService scheduleService)
        {
            if (scheduleService == null) throw new ArgumentNullException(nameof(scheduleService));

            scheduleService.RegisterSchedule("IntervalTrigger-" + Guid.NewGuid(), interval, Execute);
        }
    }
}
