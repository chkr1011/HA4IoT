using System;
using HA4IoT.Contracts.Services.Daylight;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapDaylightProvider : IDaylightProvider
    {
        public OpenWeatherMapDaylightProvider(OpenWeatherMapService openWeatherMapService)
        {
            if (openWeatherMapService == null) throw new ArgumentNullException(nameof(openWeatherMapService));

            openWeatherMapService.DaylightFetched += (s, e) => DaylightFetched?.Invoke(this, e);
        }
        
        public event EventHandler<DaylightFetchedEventArgs> DaylightFetched;
    }
}
