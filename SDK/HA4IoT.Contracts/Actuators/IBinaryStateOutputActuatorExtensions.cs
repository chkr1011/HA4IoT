using System;

namespace HA4IoT.Contracts.Actuators
{
    public static class IBinaryStateOutputActuatorExtensions
    {
        public static void TurnOff(this IBinaryStateOutputActuator actuator, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            actuator.SetState(BinaryActuatorState.Off, parameters);
        }

        public static void TurnOn(this IBinaryStateOutputActuator actuator, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            actuator.SetState(BinaryActuatorState.On, parameters);
        }
    }
}
