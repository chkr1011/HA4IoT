using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Sensors.Triggers;

namespace HA4IoT.Sensors.TemperatureSensors
{
    public static class TemperatureSensorExtensions
    {
        public static ITrigger GetTemperatureReachedTrigger(this ITemperatureSensor sensor, float target, float delta)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return new SensorValueReachedTrigger(sensor).WithTarget(target).WithDelta(delta);
        }

        public static ITrigger GetTemperatureUnderranTrigger(this ITemperatureSensor sensor, float target, float delta)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return new SensorValueUnderranTrigger(sensor).WithTarget(target).WithDelta(delta);
        }

        public static IArea WithTemperatureSensor(this IArea room, Enum id, INumericValueSensorEndpoint endpoint)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            room.AddComponent(new TemperatureSensor(ComponentIdFactory.Create(room, id), endpoint));
            return room;
        }
        
        public static ITemperatureSensor GetTemperatureSensor(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetComponent<ITemperatureSensor>(ComponentIdFactory.Create(room, id));
        }
    }
}
