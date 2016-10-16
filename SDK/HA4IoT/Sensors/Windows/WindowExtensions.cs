using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Sensors.Windows
{
    public static class WindowExtensions
    {
        public static Window GetWindow(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<Window>(ComponentIdGenerator.Generate(area.Id, id));
        }
    }
}
