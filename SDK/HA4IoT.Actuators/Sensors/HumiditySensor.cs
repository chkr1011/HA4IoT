using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public class HumiditySensor : SingleValueSensorActuatorBase<ActuatorSettings>, IHumiditySensor
    {
        public HumiditySensor(ActuatorId id, ISingleValueSensor sensor, IApiController apiController, ILogger logger)
            : base(id, apiController, logger)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            Settings = new ActuatorSettings(id, logger);

            // TODO: Move delta to settings.
            ValueChangedMinDelta = 2.5F;

            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);
        }
    }
}