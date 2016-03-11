using System;

namespace HA4IoT.Contracts.Actuators
{
    public static class IBinaryStateOutputActuatorExtensions
    {
        public static void Toggle(this IBinaryStateOutputActuator actuator, params IParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            if (actuator.GetState() == BinaryActuatorState.Off)
            {
                actuator.TurnOn();
            }
            else if (actuator.GetState() == BinaryActuatorState.On)
            {
                actuator.TurnOff();
            }
        }

        public static void TurnOff(this IBinaryStateOutputActuator actuator, params IParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            actuator.SetState(BinaryActuatorState.Off, parameters);
        }

        public static void TurnOn(this IBinaryStateOutputActuator actuator, params IParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            actuator.SetState(BinaryActuatorState.On, parameters);
        }
    }
}
