using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.MotionDetectors
{
    public static class MotionDetectorExtensions
    {
        public static IMotionDetector GetMotionDetector(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IMotionDetector>(ComponentIdGenerator.Generate(area.Id, id));
        }

        public static IMotionDetector GetMotionDetector(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IMotionDetector>();
        }
    }
}
