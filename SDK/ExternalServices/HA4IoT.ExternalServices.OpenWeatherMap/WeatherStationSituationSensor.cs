using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services.WeatherService;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class WeatherStationSituationSensor : ActuatorBase<ActuatorSettings>, IWeatherSituationSensor
    {
        private WeatherSituation _value = WeatherSituation.Unknown;

        public WeatherStationSituationSensor(ActuatorId id, IApiController apiController) 
            : base(id, apiController)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            Settings = new ActuatorSettings(id);
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

            _value = weatherSituation;

            SituationChanged?.Invoke(this, new WeatherSituationSensorValueChangedEventArgs(_value, weatherSituation));
            ApiController.NotifyStateChanged(this);
        }
    }
}
