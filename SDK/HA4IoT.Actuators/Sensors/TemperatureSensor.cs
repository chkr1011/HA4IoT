using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public class TemperatureSensor : SingleValueSensorBase<SingleValueSensorSettings>, ITemperatureSensor
    {
        public TemperatureSensor(ActuatorId id, ISingleValueSensor sensor, IApiController apiController)
            : base(id, apiController)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            Settings = new SingleValueSensorSettings(id, 0.15F);
            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);
        }
    }
}