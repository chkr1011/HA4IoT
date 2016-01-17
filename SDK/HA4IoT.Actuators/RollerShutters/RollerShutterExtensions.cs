using System;
using System.Linq;
using HA4IoT.Actuators.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class RollerShutterExtensions
    {
        public static IRollerShutter[] GetAllRollerShutters(this IRoom room)
        {
            return room.Actuators().Where(a => a is RollerShutter).Cast<IRollerShutter>().ToArray();
        }
        
        public static IRollerShutter RollerShutter(this IRoom room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<RollerShutter>(ActuatorIdFactory.Create(room, id));
        }

        public static AutomaticRollerShutterAutomation SetupAutomaticRollerShutters(this IRoom room)
        {
            return new AutomaticRollerShutterAutomation(room.Controller.Timer, room.Controller.WeatherStation, room.Controller.Logger);
        }

        public static IRoom WithRollerShutter(this IRoom room, Enum id, IBinaryOutput powerOutput, IBinaryOutput directionOutput,
            TimeSpan autoOffTimeout, int maxPosition)
        {
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            var rollerShutter = new RollerShutter(ActuatorIdFactory.Create(room, id), powerOutput, directionOutput,
                autoOffTimeout, maxPosition, room.Controller.HttpApiController, room.Controller.Logger, room.Controller.Timer);

            room.AddActuator(rollerShutter);
            return room;
        }
    }
}
