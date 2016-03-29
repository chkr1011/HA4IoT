using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Actuators
{
    public static class WindowExtensions
    {
        public static IArea WithWindow(this IArea room, Enum id, Action<Window> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var window = new Window(ActuatorIdFactory.Create(room, id));
            initializer(window);

            room.AddActuator(window);
            return room;
        }

        public static Window Window(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetActuator<Window>(ActuatorIdFactory.Create(room, id));
        }
    }
}
