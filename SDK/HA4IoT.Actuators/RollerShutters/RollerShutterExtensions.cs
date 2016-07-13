using System;
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
        public static IRollerShutter[] GetRollerShutters(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponents<IRollerShutter>().ToArray();
        }

        public static IRollerShutter GetRollerShutter(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<RollerShutter>(ComponentIdFactory.Create(area.Id, id));
        }

        public static IArea WithRollerShutter(this IArea area, Enum id, IBinaryOutput powerOutput, IBinaryOutput directionOutput)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            var rollerShutter = new RollerShutter(
                ComponentIdFactory.Create(area.Id, id), 
                new PortBasedRollerShutterEndpoint(powerOutput, directionOutput),
                area.Controller.Timer,
                area.Controller.ServiceLocator.GetService<ISchedulerService>());

            area.AddComponent(rollerShutter);
            return area;
        }
    }
}
