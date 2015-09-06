using System;

namespace CK.HomeAutomation.Actuators.Connectors
{
    public static class BinaryStateActuatorWithButtonConnector
    {
        public static IBinaryStateOutputActuator ConnectToggleWith(this IBinaryStateOutputActuator actuator, Button button)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.PressedShort += (s, e) => actuator.Toggle();

            return actuator;
        }
    }
}
