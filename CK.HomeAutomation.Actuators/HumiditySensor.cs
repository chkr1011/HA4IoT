using System;
using CK.HomeAutomation.Hardware.Drivers;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class HumiditySensor : BaseSensor
    {
        public HumiditySensor(string id, int sensorId, TemperatureAndHumiditySensorBridgeDriver driver,
            HttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
            if (driver == null) throw new ArgumentNullException(nameof(driver));

            driver.ValuesUpdated += (s, e) => UpdateValue(driver.GetHumidity(sensorId));
        }
    }
}