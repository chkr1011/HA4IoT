using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.WeatherStation
{
    public interface IWeatherStation : IDevice, IStatusProvider
    {
        Daylight Daylight { get; }

        IWeatherSituationSensor SituationSensor { get; }
        ITemperatureSensor TemperatureSensor { get; }
        IHumiditySensor HumiditySensor { get; }
    }
}