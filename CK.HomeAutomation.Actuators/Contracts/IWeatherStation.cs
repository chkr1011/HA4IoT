using Windows.Data.Json;

namespace CK.HomeAutomation.Actuators.Contracts
{
    public interface IWeatherStation
    {
        Daylight Daylight { get; }
        ITemperatureSensor Temperature { get; }
        IHumiditySensor Humidity { get; }
        JsonObject ApiGet();
    }
}