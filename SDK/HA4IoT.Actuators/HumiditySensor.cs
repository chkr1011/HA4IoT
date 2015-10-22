using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public class HumiditySensor : SingleValueSensorBase, IHumiditySensor
    {
        public HumiditySensor(string id, ISingleValueSensor sensor,
            IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            sensor.ValueChanged += (s, e) => UpdateValue(e.NewValue);
        }
    }
}