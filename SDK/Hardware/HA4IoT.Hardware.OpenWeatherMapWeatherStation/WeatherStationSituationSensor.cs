using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationSituationSensor : ActuatorBase<ActuatorSettings>, IWeatherSituationSensor
    {
        private WeatherSituation _value = WeatherSituation.Unknown;

        public WeatherStationSituationSensor(ActuatorId id, IApiController apiController, ILogger logger) 
            : base(id, apiController, logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));
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
