using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators
{
    public static class ComponentExtensions
    {
        public static void Enable(this IComponent actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            actuator.GeneralSettingsWrapper.IsEnabled = true;
        }

        public static void Disable(this IComponent actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            actuator.GeneralSettingsWrapper.IsEnabled = false;
        }

        public static bool GetIsEnabled(this IComponent actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            return actuator.GeneralSettingsWrapper.IsEnabled;
        }
    }
}
