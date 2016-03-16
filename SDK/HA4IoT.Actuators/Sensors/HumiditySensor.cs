using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public class HumiditySensor : SingleValueSensorBase<SingleValueSensorSettings>, IHumiditySensor
    {
        public HumiditySensor(ActuatorId id, ISingleValueSensor sensor, IApiController apiController)
            : base(id, apiController)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            Settings = new SingleValueSensorSettings(id, 2.5F);
            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);
        }
    }
}