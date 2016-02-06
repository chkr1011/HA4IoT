using System;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class LampExtensions
    {
        public static IRoom WithLamp(this IRoom room, Enum id, IBinaryOutput output)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var lamp = new Lamp(ActuatorIdFactory.Create(room, id), output, room.Controller.HttpApiController, room.Controller.Logger);
            lamp.SetInitialState();

            room.AddActuator(lamp);
            return room;
        }

        public static Lamp Lamp(this IRoom room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<Lamp>(ActuatorIdFactory.Create(room, id));
        }
    }
}
