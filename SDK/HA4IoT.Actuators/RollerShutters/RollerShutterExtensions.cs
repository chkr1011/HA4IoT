using System;
using System.Linq;
using HA4IoT.Actuators.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class RollerShutterExtensions
    {
        public static IRollerShutter[] GetAllRollerShutters(this Room room)
        {
            return room.Actuators.Where(a => a is RollerShutter).Cast<IRollerShutter>().ToArray();
        }
        
        public static IRollerShutter RollerShutter(this Room room, Enum id)
        {
            return room.Actuator<RollerShutter>(id);
        }

        public static AutomaticRollerShutterAutomation SetupAutomaticRollerShutters(this Room room)
        {
            return new AutomaticRollerShutterAutomation(room.Home.Timer, room.Home.WeatherStation, room.Home.Log);
        }

        public static Room WithRollerShutter(this Room room, Enum id, IBinaryOutput powerOutput, IBinaryOutput directionOutput,
            TimeSpan autoOffTimeout, int maxPosition)
        {
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            return room.WithActuator(id,
                new RollerShutter(room.GenerateActuatorId(id), powerOutput, directionOutput, autoOffTimeout, maxPosition,
                    room.Home.Api, room.Home.Log, room.Home.Timer));
        }
    }
}
