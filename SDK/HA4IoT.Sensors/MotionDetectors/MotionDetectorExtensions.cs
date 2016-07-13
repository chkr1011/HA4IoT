using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Sensors.MotionDetectors
{
    public static class MotionDetectorExtensions
    {
        public static IArea WithMotionDetector(this IArea area, Enum id, IBinaryInput input)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var motionDetector = new MotionDetector(
                ComponentIdFactory.Create(area.Id, id), 
                new PortBasedMotionDetectorEndpoint(input), 
                area.Controller.ServiceLocator.GetService<ISchedulerService>());

            area.AddComponent(motionDetector);
            return area;
        }

        public static IMotionDetector GetMotionDetector(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IMotionDetector>(ComponentIdFactory.Create(area.Id, id));
        }

        public static IMotionDetector GetMotionDetector(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IMotionDetector>();
        }
    }
}
