using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.Buttons
{
    public static class ButtonExtensions
    {
        public static IButton GetButton(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IButton>($"{area.Id}.{id}");
        }
    }
}
