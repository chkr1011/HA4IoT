using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.Lamps
{
    public static class LampExtensions
    {
        public static IArea WithLamp(this IArea area, Enum id, IBinaryOutput output)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var lamp = new Lamp(ComponentIdFactory.Create(area.Id, id), new PortBasedBinaryStateEndpoint(output));
            area.AddComponent(lamp);

            return area;
        }

        public static ILamp GetLamp(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<ILamp>(ComponentIdFactory.Create(area.Id, id));
        }
    }
}
