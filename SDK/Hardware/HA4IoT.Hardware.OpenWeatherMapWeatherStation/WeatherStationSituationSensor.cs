using System;
using Windows.Data.Json;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationSituationSensor : IWeatherSituationSensor
    {
        private WeatherSituation _value = WeatherSituation.Unknown;

        public event EventHandler<WeatherSituationSensorValueChangedEventArgs> SituationChanged;

        public WeatherSituation GetSituation()
        {
            return _value;
        }

        public void SetValue(JsonValue id)
        {
            WeatherSituation newValue = new WeatherSituationParser().Parse(id);
            
            if (newValue == _value)
            {
                return;
            }

            SituationChanged?.Invoke(this, new WeatherSituationSensorValueChangedEventArgs(_value, newValue));
            _value = newValue;
        }
    }
}
