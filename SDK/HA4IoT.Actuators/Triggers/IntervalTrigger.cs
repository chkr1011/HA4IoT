using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Actuators.Triggers
{
    public class IntervalTrigger : Trigger
    {
        public IntervalTrigger(TimeSpan interval, ISchedulerService scheduleService)
        {
            if (scheduleService == null) throw new ArgumentNullException(nameof(scheduleService));

            scheduleService.Every(interval).Do(Execute);
        }
    }
}
