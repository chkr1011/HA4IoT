using Windows.Data.Json;

namespace CK.HomeAutomation.Actuators.Contracts
{
    public interface IWeatherStation
    {
        Daylight Daylight { get; }
        float Temperature { get; }
        float Humidity { get; }
        JsonObject ApiGet();
    }
}