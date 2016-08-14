using System;
using HA4IoT.Contracts.Services.OutdoorHumidity;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapOutdoorHumidityProvider : IOutdoorHumidityProvider
    {
        public OpenWeatherMapOutdoorHumidityProvider(OpenWeatherMapService openWeatherMapService)
        {
            if (openWeatherMapService == null) throw new ArgumentNullException(nameof(openWeatherMapService));

            openWeatherMapService.OutdoorHumidityFetched += (s, e) => OutdoorHumidityFetched?.Invoke(this, e);
        }

        public event EventHandler<OutdoorHumidityFetchedEventArgs> OutdoorHumidityFetched;
    }
}
