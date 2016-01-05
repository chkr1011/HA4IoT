using System;

namespace HA4IoT.Contracts.WeatherStation
{
    public interface IWeatherSituationSensor
    {
        event EventHandler<WeatherSituationSensorValueChangedEventArgs> SituationChanged;

        WeatherSituation GetSituation();
    }
}
