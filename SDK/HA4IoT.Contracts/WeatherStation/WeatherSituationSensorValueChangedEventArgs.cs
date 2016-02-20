using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.WeatherStation
{
    public class WeatherSituationSensorValueChangedEventArgs : ValueChangedEventArgs<WeatherSituation>
    {
        public WeatherSituationSensorValueChangedEventArgs(WeatherSituation oldValue, WeatherSituation newValue) : base(oldValue, newValue)
        {
        }
    }
}
