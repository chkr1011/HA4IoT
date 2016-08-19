using System;

namespace HA4IoT.Contracts.Services.Weather
{
    public interface IWeatherService : IService
    {
        Weather Weather { get; }

        DateTime? Timestamp { get; }
    }
}
