using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Actuators.Triggers
{
    public class IntervalTrigger : Trigger
    {
        public IntervalTrigger(IHomeAutomationTimer timer, TimeSpan interval)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            timer.Every(interval).Do(Invoke);
        }
    }
}
