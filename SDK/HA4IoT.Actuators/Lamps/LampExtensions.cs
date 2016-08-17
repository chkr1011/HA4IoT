using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.Lamps
{
    public static class LampExtensions
    {
        public static ILamp GetLamp(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<ILamp>(ComponentIdFactory.Create(area.Id, id));
        }
    }
}
