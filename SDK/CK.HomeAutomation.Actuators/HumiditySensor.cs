using System;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
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