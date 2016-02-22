using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Actuators
{
    public class HumiditySensor : SingleValueSensorActuatorBase<ActuatorSettings>, IHumiditySensor
    {
        public HumiditySensor(ActuatorId id, ISingleValueSensor sensor, IHttpRequestController api, ILogger logger)
            : base(id, api, logger)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            Settings = new ActuatorSettings(id, logger);

            // TODO: Move delta to settings.
            ValueChangedMinDelta = 2.5F;

            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);
        }
    }
}