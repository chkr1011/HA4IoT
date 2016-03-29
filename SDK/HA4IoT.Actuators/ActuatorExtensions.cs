using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators
{
    public static class ActuatorExtensions
    {
        public static void Enable(this IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            actuator.GeneralSettingsWrapper.IsEnabled = true;
        }

        public static void Disable(this IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            actuator.GeneralSettingsWrapper.IsEnabled = false;
        }

        public static bool GetIsEnabled(this IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            return actuator.GeneralSettingsWrapper.IsEnabled;
        }
    }
}
