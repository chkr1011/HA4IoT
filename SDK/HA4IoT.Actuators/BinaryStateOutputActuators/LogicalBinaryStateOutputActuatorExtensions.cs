using System;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Actuators
{
    public static class LogicalBinaryStateOutputActuatorExtensions
    {
        public static LogicalBinaryStateOutputActuator CombineActuators(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            var actuator = new LogicalBinaryStateOutputActuator(ActuatorIdFactory.Create(room, id), room.Controller.HttpApiController, room.Controller.Logger, room.Controller.Timer);
            room.AddActuator(actuator);
            return actuator;
        }
    }
}
