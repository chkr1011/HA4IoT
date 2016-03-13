using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public class HumiditySensor : SingleValueSensorBase<SingleValueSensorSettings>, IHumiditySensor
    {
        public HumiditySensor(ActuatorId id, ISingleValueSensor sensor, IApiController apiController, ILogger logger)
            : base(id, apiController, logger)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            Settings = new SingleValueSensorSettings(id, 2.5F, logger);
            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);
        }
    }
}