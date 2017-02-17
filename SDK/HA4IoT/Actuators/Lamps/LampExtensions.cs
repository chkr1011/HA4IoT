using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Actuators.Lamps
{
    public static class LampExtensions
    {
        public static ILamp GetLamp(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<ILamp>($"{area.Id}.{id}");
        }
    }
}
