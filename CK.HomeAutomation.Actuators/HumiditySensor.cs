using System;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class HumiditySensor : BaseSensor
    {
        public HumiditySensor(string id, int sensorId, DHT22Reader dht22Reader,
            HttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
            if (dht22Reader == null) throw new ArgumentNullException(nameof(dht22Reader));

            dht22Reader.ValuesUpdated += (s, e) => UpdateValue(dht22Reader.GetHumidity(sensorId));
        }
    }
}