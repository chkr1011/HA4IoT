using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators
{
    public static class HumiditySensorExtensions
    {
        public static ITrigger GetHumidityReachedTrigger(this IHumiditySensor sensor, float value, float delta = 5)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return new SensorValueReachedTrigger(sensor).WithTarget(value).WithDelta(delta);
        }

        public static ITrigger GetHumidityUnderranTrigger(this IHumiditySensor sensor, float value, float delta = 5)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return new SensorValueUnderranTrigger(sensor).WithTarget(value).WithDelta(delta);
        }

        public static IArea WithHumiditySensor(this IArea room, Enum id, ISingleValueSensor sensor)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            room.AddActuator(new HumiditySensor(ActuatorIdFactory.Create(room, id), sensor, room.Controller.ApiController));
            return room;
        }

        public static IHumiditySensor GetHumiditySensor(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetActuator<IHumiditySensor>(ActuatorIdFactory.Create(room, id));
        }
    }
}
