using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators.Actions
{
    public static class HomeAutomationActionExtensions
    {
        public static IAction OnTrigger(this IAction actuatorAction, ITrigger trigger)
        {
            if (actuatorAction == null) throw new ArgumentNullException(nameof(actuatorAction));
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Attach(actuatorAction);
            return actuatorAction;
        }

        public static IAction ToHomeAutomationAction(System.Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            return new Action(callback);
        }
    }
}
