using System;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Actuators
{
    public static class WindowExtensions
    {
        public static IRoom WithWindow(this IRoom room, Enum id, Action<Window> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var window = new Window(ActuatorIdFactory.Create(room, id), room.Controller.HttpApiController, room.Controller.Logger);
            initializer(window);

            room.AddActuator(window);
            return room;
        }

        public static Window Window(this IRoom room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<Window>(ActuatorIdFactory.Create(room, id));
        }
    }
}
