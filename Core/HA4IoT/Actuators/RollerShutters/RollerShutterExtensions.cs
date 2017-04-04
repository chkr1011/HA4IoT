using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Actuators.RollerShutters
{
    public static class RollerShutterExtensions
    {
        public static IRollerShutter GetRollerShutter(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<RollerShutter>($"{area.Id}.{id}");
        }
    }
}
