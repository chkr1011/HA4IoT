using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationTemperatureSensor : SingleValueSensorActuatorBase, ITemperatureSensor
    {
        public WeatherStationTemperatureSensor(ActuatorId id, IHttpRequestController api, INotificationHandler log) 
            : base(id, api, log)
        {
        }

        public void SetValue(double value)
        {
            SetValueInternal(Convert.ToSingle(value));
        }
    }
}
