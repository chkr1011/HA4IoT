using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public class TemperatureSensor : SingleValueSensorActuatorBase<ActuatorSettings>, ITemperatureSensor
    {
        public TemperatureSensor(ActuatorId id, ISingleValueSensor sensor, IApiController apiController, ILogger logger)
            : base(id, apiController, logger)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);

            // TODO: Add delta to settings.
            Settings = new ActuatorSettings(id, logger);
        }
    }
}