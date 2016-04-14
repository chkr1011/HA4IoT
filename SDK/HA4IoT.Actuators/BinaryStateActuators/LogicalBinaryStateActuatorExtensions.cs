using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public static class LogicalBinaryStateActuatorExtensions
    {
        public static LogicalBinaryStateActuator CombineActuators(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            var actuator = new LogicalBinaryStateActuator(ComponentIdFactory.Create(room, id), room.Controller.Timer);
            room.AddComponent(actuator);
            return actuator;
        }
    }
}
