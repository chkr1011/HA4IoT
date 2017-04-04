using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Actuators.Fans
{
    public static class FanExtensions
    {
        public static IFan GetFan(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IFan>($"{area.Id}.{id}");
        }
    }
}
