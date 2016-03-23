using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Services.WeatherService
{
    public interface IWeatherService : IService, IStatusProvider
    {
        // TODO: Consider split into ExternalTemperatureService, ExternalHumidityService, ExternalWeatherSituationService
        IWeatherSituationSensor SituationSensor { get; }
        ITemperatureSensor TemperatureSensor { get; }
        IHumiditySensor HumiditySensor { get; }
    }
}