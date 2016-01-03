using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.WeatherStation
{
    public interface IWeatherStation : IStatusProvider
    {
        Daylight Daylight { get; }

        IWeatherSituationSensor SituationSensor { get; }
        ITemperatureSensor TemperatureSensor { get; }
        IHumiditySensor HumiditySensor { get; }
    }
}