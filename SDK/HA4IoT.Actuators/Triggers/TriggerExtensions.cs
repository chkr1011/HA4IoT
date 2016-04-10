using System;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators.Triggers
{
    public static class TriggerExtensions
    {
        public static ITrigger Attach(this ITrigger trigger, IAction actuatorAction)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            if (actuatorAction == null) throw new ArgumentNullException(nameof(actuatorAction));

            trigger.Attach(actuatorAction.Execute);
            return trigger;
        }
    }
}
