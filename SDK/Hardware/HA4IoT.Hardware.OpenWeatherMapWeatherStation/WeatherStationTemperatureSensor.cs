using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationTemperatureSensor : SingleValueSensorBase<SingleValueSensorSettings>, ITemperatureSensor
    {
        public WeatherStationTemperatureSensor(ActuatorId id, IApiController apiController, ILogger logger) 
            : base(id, apiController, logger)
        {
            Settings = new SingleValueSensorSettings(id, 0.15F, logger);
        }

        public void SetValue(double value)
        {
            SetValueInternal(Convert.ToSingle(value));
        }
    }
}
