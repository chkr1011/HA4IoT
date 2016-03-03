using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Actuators
{
    public class TemperatureSensor : SingleValueSensorActuatorBase<ActuatorSettings>, ITemperatureSensor
    {
        public TemperatureSensor(ActuatorId id, ISingleValueSensor sensor, IHttpRequestController api, ILogger logger)
            : base(id, api, logger)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);

            // TODO: Add delta to settings.
            Settings = new ActuatorSettings(id, logger);
        }
    }
}