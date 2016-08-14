using System;
using HA4IoT.Contracts.Services.OutdoorTemperature;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapOutdoorTemperatureProvider : IOutdoorTemperatureProvider
    {
        public OpenWeatherMapOutdoorTemperatureProvider(OpenWeatherMapService openWeatherMapService)
        {
            if (openWeatherMapService == null) throw new ArgumentNullException(nameof(openWeatherMapService));

            openWeatherMapService.OutdoorTemperatureFetched += (s, e) => OutdoorTemperatureFetched?.Invoke(this, e);
        }

        public event EventHandler<OutdoorTemperatureFetchedEventArgs> OutdoorTemperatureFetched;
    }
}
