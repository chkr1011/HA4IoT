using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationSituationSensor : ActuatorBase<ActuatorSettings>, IWeatherSituationSensor
    {
        private WeatherSituation _value = WeatherSituation.Unknown;

        public WeatherStationSituationSensor(ActuatorId id, IHttpRequestController httpApiController, ILogger logger) 
            : base(id, httpApiController, logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Settings = new ActuatorSettings(id, logger);
        }

        public event EventHandler<WeatherSituationSensorValueChangedEventArgs> SituationChanged;

        public WeatherSituation GetSituation()
        {
            return _value;
        }

        public void SetValue(WeatherSituation weatherSituation)
        {
            if (weatherSituation == _value)
            {
                return;
            }

            SituationChanged?.Invoke(this, new WeatherSituationSensorValueChangedEventArgs(_value, weatherSituation));
            _value = weatherSituation;
        }
    }
}
