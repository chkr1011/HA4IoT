using System;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class HumiditySensor : SingleValueSensorActuatorBase, IHumiditySensor
    {
        public HumiditySensor(ActuatorId id, ISingleValueSensor sensor, IHttpRequestController api, INotificationHandler logger)
            : base(id, api, logger)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            ValueChangedMinDelta = 2.5F;

            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);
        }
    }
}