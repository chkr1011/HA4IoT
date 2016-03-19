using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class MotionDetectorExtensions
    {
        public static IArea WithMotionDetector(this IArea room, Enum id, IBinaryInput input)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var motionDetector = new MotionDetector(
                ActuatorIdFactory.Create(room, id), 
                new PortBasedMotionDetectorEndpoint(input), 
                room.Controller.Timer, 
                room.Controller.ApiController);

            room.AddActuator(motionDetector);
            return room;
        }

        public static IMotionDetector GetMotionDetector(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetActuator<IMotionDetector>(ActuatorIdFactory.Create(room, id));
        }

        public static IMotionDetector GetMotionDetector(this IArea room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetActuator<IMotionDetector>();
        }
    }
}
