using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Actuators.RollerShutters
{
    public static class RollerShutterExtensions
    {
        public static IList<IRollerShutter> GetRollerShutters(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponents<IRollerShutter>().ToArray();
        }

        public static IRollerShutter GetRollerShutter(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<RollerShutter>(ComponentIdFactory.Create(area.Id, id));
        }
    }
}
