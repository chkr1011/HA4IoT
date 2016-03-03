using System;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class RollerShutterExtensions
    {
        public static IRollerShutter[] GetAllRollerShutters(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.Actuators<IRollerShutter>().ToArray();
        }

        public static IRollerShutter RollerShutter(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.Actuator<RollerShutter>(ActuatorIdFactory.Create(area, id));
        }

        public static IArea WithRollerShutter(this IArea area, Enum id, IBinaryOutput powerOutput, IBinaryOutput directionOutput)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            var rollerShutter = new RollerShutter(ActuatorIdFactory.Create(area, id), powerOutput, directionOutput,
                area.Controller.HttpApiController, area.Controller.Logger, area.Controller.Timer);

            area.AddActuator(rollerShutter);
            return area;
        }
    }
}
