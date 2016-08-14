using System;
using HA4IoT.Contracts.Services.Weather;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapWeatherProvider : IWeatherProvider
    {
        public OpenWeatherMapWeatherProvider(OpenWeatherMapService openWeatherMapService)
        {
            if (openWeatherMapService == null) throw new ArgumentNullException(nameof(openWeatherMapService));

            openWeatherMapService.WeatherFetched += (s, e) => WeatherFetched?.Invoke(this, e);
        }

        public event EventHandler<WeatherFetchedEventArgs> WeatherFetched;
    }
}
