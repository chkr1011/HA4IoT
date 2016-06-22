using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Actuators
{
    public static class ActuatorExtensions
    {
        public static IActuator GetActuator(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IActuator>(ComponentIdFactory.Create(area.Id, id));
        }

        public static IActuator GetActuator(this IComponentController controller, ComponentId id)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (id == null) throw new ArgumentNullException(nameof(id));

            return controller.GetComponent<IActuator>(id);
        }

        public static bool IsOn(this IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            return actuator.GetState().Equals(BinaryStateId.On);
        }

        public static bool IsOff(this IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            return actuator.GetState().Equals(BinaryStateId.On);
        }
    }
}
