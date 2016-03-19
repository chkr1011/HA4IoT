using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationHumiditySensor : SingleValueSensorBase<SingleValueSensorSettings>, IHumiditySensor
    {
        public WeatherStationHumiditySensor(ActuatorId id, IApiController apiController) 
            : base(id, apiController)
        {
            Settings = new SingleValueSensorSettings(id, 2.5F);
        }

        public void SetValue(double value)
        {
            SetValueInternal(Convert.ToSingle(value));
        }
    }
}
