using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class TemperatureSensor : SingleValueSensorActuatorBase, ITemperatureSensor
    {
        public TemperatureSensor(string id, ISingleValueSensor sensor,
            IHttpRequestController api, INotificationHandler log)
            : base(id, api, log)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);
        }
    }
}