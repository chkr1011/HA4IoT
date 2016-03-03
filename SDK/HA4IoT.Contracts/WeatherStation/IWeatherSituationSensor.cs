using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.WeatherStation
{
    public interface IWeatherSituationSensor : IActuator
    {
        event EventHandler<WeatherSituationSensorValueChangedEventArgs> SituationChanged;

        WeatherSituation GetSituation();
    }
}
