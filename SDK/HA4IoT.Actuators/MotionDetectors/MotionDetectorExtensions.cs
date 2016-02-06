using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class MotionDetectorExtensions
    {
        public static IRoom WithMotionDetector(this IRoom room, Enum id, IBinaryInput input)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var motionDetector = new MotionDetector(ActuatorIdFactory.Create(room, id), input, room.Controller.Timer, room.Controller.HttpApiController, room.Controller.Logger);
            room.AddActuator(motionDetector);
            return room;
        }

        public static IMotionDetector MotionDetector(this IRoom room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<MotionDetector>(ActuatorIdFactory.Create(room, id));
        }
    }
}
