using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators.Actions
{
    public static class HomeAutomationActionExtensions
    {
        public static IHomeAutomationAction AssociateWith(this IHomeAutomationAction actuatorAction, ITrigger trigger)
        {
            if (actuatorAction == null) throw new ArgumentNullException(nameof(actuatorAction));
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.AssociateWith(actuatorAction);
            return actuatorAction;
        }

        public static IHomeAutomationAction ToHomeAutomationAction(Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            return new HomeAutomationAction(callback);
        }
    }
}
