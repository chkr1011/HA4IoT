using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Actuators
{
    public static class ActuatorExtensions
    {
        public static IActuator GetActuator(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetComponent<IActuator>(ComponentIdFactory.Create(room, id));
        }

        public static IActuator GetActuator(this IComponentController controller, ComponentId id)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (id == null) throw new ArgumentNullException(nameof(id));

            return controller.GetComponent<IActuator>(id);
        }

        public static bool GetIsOn(this IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            return actuator.GetState().Equals(BinaryStateId.On);
        }

        public static bool GetIsOff(this IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            return actuator.GetState().Equals(BinaryStateId.On);
        }
    }
}
