using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators.Actions
{
    public static class ActionExtensions
    {
        public static IAction OnTrigger(this IAction action, ITrigger trigger)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Attach(action);
            return action;
        }

        public static IAction ToHomeAutomationAction(System.Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            return new Action(callback);
        }
    }
}
