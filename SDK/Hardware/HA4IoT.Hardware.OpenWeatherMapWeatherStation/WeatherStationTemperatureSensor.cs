using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationTemperatureSensor : SingleValueSensorBase<SingleValueSensorSettings>, ITemperatureSensor
    {
        public WeatherStationTemperatureSensor(ActuatorId id, IApiController apiController) 
            : base(id, apiController)
        {
            Settings = new SingleValueSensorSettings(id, 0.15F);
        }

        public void SetValue(double value)
        {
            SetValueInternal(Convert.ToSingle(value));
        }
    }
}
