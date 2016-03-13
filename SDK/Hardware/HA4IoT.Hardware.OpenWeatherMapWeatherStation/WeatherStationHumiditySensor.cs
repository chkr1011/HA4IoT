using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationHumiditySensor : SingleValueSensorBase<SingleValueSensorSettings>, IHumiditySensor
    {
        public WeatherStationHumiditySensor(ActuatorId id, IApiController apiController, ILogger logger) 
            : base(id, apiController, logger)
        {
            Settings = new SingleValueSensorSettings(id, 2.5F, logger);
        }

        public void SetValue(double value)
        {
            SetValueInternal(Convert.ToSingle(value));
        }
    }
}
