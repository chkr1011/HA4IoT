using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationHumiditySensor : SingleValueSensorActuatorBase, IHumiditySensor
    {
        public WeatherStationHumiditySensor(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler) : base(id, httpApiController, notificationHandler)
        {
        }
    }
}
