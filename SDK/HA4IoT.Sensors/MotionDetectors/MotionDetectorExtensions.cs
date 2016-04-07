using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.MotionDetectors
{
    public static class MotionDetectorExtensions
    {
        public static IArea WithMotionDetector(this IArea room, Enum id, IBinaryInput input)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var motionDetector = new MotionDetector(
                ComponentIdFactory.Create(room, id), 
                new PortBasedMotionDetectorEndpoint(input), 
                room.Controller.Timer);

            room.AddComponent(motionDetector);
            return room;
        }

        public static IMotionDetector GetMotionDetector(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetComponent<IMotionDetector>(ComponentIdFactory.Create(room, id));
        }

        public static IMotionDetector GetMotionDetector(this IArea room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetComponent<IMotionDetector>();
        }
    }
}
