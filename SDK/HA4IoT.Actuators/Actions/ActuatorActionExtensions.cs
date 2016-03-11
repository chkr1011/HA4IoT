using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators.Actions
{
    public static class ActuatorActionExtensions
    {
        public static IActuatorAction AssociateWith(this IActuatorAction actuatorAction, ITrigger trigger)
        {
            if (actuatorAction == null) throw new ArgumentNullException(nameof(actuatorAction));
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.AssociateWith(actuatorAction);
            return actuatorAction;
        }
    }
}
