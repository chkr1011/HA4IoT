using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Environment
{
    public interface IOutdoorService : IService
    {
        float Humidity { get; }
        DateTime? HumidityTimestamp { get; }
        
        float Temperature { get; }
        DateTime? TemperatureTimestamp { get; }

        WeatherCondition Condition { get; }
        DateTime? ConditionTimestamp { get; }

        void UpdateHumidity(float value);
        void UpdateTemperature(float value);
        void UpdateCondition(WeatherCondition value);
    }
}
