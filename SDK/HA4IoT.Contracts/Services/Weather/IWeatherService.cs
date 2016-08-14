using System;

namespace HA4IoT.Contracts.Services.Weather
{
    public interface IWeatherService : IApiExposedService
    {
        Weather Weather { get; }

        DateTime? Timestamp { get; }
    }
}
