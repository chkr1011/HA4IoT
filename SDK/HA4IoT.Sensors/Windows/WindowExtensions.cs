using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Sensors.Windows
{
    public static class WindowExtensions
    {
        public static IArea WithWindow(this IArea room, Enum id, Action<Window> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var window = new Window(ComponentIdFactory.Create(room, id));
            initializer(window);

            room.AddComponent(window);
            return room;
        }

        public static Window Window(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetComponent<Window>(ComponentIdFactory.Create(room, id));
        }
    }
}
