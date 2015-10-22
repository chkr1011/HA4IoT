using Windows.Data.Json;

namespace HA4IoT.Contracts.Actuators
{
    public interface IWeatherStation
    {
        Daylight Daylight { get; }
        ITemperatureSensor Temperature { get; }
        IHumiditySensor Humidity { get; }
        JsonObject ApiGet();
    }
}