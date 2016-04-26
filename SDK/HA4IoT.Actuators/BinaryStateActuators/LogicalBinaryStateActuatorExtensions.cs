using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public static class LogicalBinaryStateActuatorExtensions
    {
        public static LogicalBinaryStateActuator CombineActuators(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var actuator = new LogicalBinaryStateActuator(ComponentIdFactory.Create(area.Id, id), area.Controller.Timer);
            area.AddComponent(actuator);
            return actuator;
        }
    }
}
