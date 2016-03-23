namespace HA4IoT.Contracts.Services.WeatherService
{
    public class WeatherSituationSensorValueChangedEventArgs : ValueChangedEventArgs<WeatherSituation>
    {
        public WeatherSituationSensorValueChangedEventArgs(WeatherSituation oldValue, WeatherSituation newValue) : base(oldValue, newValue)
        {
        }
    }
}
