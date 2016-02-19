using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Actuators
{
    public static class BinaryStateOutputActuatorExtensions
    {
        public static IBinaryStateOutputActuator BinaryStateOutput(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return (IBinaryStateOutputActuator)room.Actuator<ActuatorBase>(ActuatorIdFactory.Create(room, id));
        }
    }
}
