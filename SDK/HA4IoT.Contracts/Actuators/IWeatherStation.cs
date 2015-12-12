using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Actuators
{
    public interface IWeatherStation : IStatusProvider
    {
        Daylight Daylight { get; }
        ITemperatureSensor TemperatureSensor { get; }
        IHumiditySensor HumiditySensor { get; }
    }
}