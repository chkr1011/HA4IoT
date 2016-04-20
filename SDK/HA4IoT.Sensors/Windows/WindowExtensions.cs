using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Sensors.Windows
{
    public static class WindowExtensions
    {
        public static IArea WithWindow(this IArea area, Enum id, Action<Window> initializer)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var window = new Window(ComponentIdFactory.Create(area.Id, id));
            initializer(window);

            area.AddComponent(window);
            return area;
        }

        public static Window Window(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<Window>(ComponentIdFactory.Create(area.Id, id));
        }
    }
}
