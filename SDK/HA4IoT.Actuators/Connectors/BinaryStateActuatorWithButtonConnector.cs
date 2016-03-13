using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators.Connectors
{
    public static class BinaryStateActuatorWithButtonConnector
    {
        public static IBinaryStateOutputActuator ConnectToggleActionWith(this IBinaryStateOutputActuator actuator, IButton button, ButtonPressedDuration pressedDuration = ButtonPressedDuration.Short)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));
            if (button == null) throw new ArgumentNullException(nameof(button));

            ConnectToggleAction(button, actuator, pressedDuration);

            return actuator;
        }

        public static IButton ConnectToggleActionWith(this IButton button, IBinaryStateOutputActuator actuator, ButtonPressedDuration pressedDuration = ButtonPressedDuration.Short)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));
            if (button == null) throw new ArgumentNullException(nameof(button));

            ConnectToggleAction(button, actuator, pressedDuration);

            return button;
        }

        private static void ConnectToggleAction(IButton button, IBinaryStateOutputActuator actuator, ButtonPressedDuration pressedDuration)
        {
            if (pressedDuration == ButtonPressedDuration.Short)
            {
                button.GetPressedShortlyTrigger().OnTriggered(actuator.GetToggleStateAction());
            }
            else if (pressedDuration == ButtonPressedDuration.Long)
            {
                button.GetPressedLongTrigger().OnTriggered(actuator.GetToggleStateAction());
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
