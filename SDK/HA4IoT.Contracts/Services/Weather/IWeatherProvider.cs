using System;

namespace HA4IoT.Contracts.Services.Weather
{
    public interface IWeatherProvider
    {
        event EventHandler<WeatherFetchedEventArgs> WeatherFetched;
    }
}
