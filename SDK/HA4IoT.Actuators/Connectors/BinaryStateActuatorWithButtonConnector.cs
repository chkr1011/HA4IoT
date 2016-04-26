using System;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Actuators.Connectors
{
    public static class BinaryStateActuatorWithButtonConnector
    {
        public static IStateMachine ConnectToggleActionWith(this IStateMachine actuator, IButton button, ButtonPressedDuration pressedDuration = ButtonPressedDuration.Short)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));
            if (button == null) throw new ArgumentNullException(nameof(button));

            ConnectToggleAction(button, actuator, pressedDuration);

            return actuator;
        }

        public static IButton ConnectToggleActionWith(this IButton button, IStateMachine actuator, ButtonPressedDuration pressedDuration = ButtonPressedDuration.Short)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));
            if (button == null) throw new ArgumentNullException(nameof(button));

            ConnectToggleAction(button, actuator, pressedDuration);

            return button;
        }

        private static void ConnectToggleAction(IButton button, IStateMachine actuator, ButtonPressedDuration pressedDuration)
        {
            if (pressedDuration == ButtonPressedDuration.Short)
            {
                button.GetPressedShortlyTrigger().Attach(actuator.GetSetNextStateAction());
            }
            else if (pressedDuration == ButtonPressedDuration.Long)
            {
                button.GetPressedLongTrigger().Attach(actuator.GetSetNextStateAction());
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
