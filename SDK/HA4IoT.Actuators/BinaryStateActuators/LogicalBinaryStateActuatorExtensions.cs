using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Actuators
{
    public static class LogicalBinaryStateActuatorExtensions
    {
        public static LogicalBinaryStateActuator CombineActuators(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            var actuator = new LogicalBinaryStateActuator(ActuatorIdFactory.Create(room, id), room.Controller.Timer);
            room.AddActuator(actuator);
            return actuator;
        }
    }
}
