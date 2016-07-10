using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Services;
using HA4IoT.Networking;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapDaylightService : ServiceBase, IDaylightService
    {
        private TimeSpan _sunrise;
        private TimeSpan _sunset;

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedTimeSpan("Sunrise", _sunrise);
            status.SetNamedTimeSpan("Sunset", _sunset);

            return status;
        }

        public TimeSpan GetSunrise()
        {
            return _sunrise;
        }

        public TimeSpan GetSunset()
        {
            return _sunset;
        }

        internal void Update(TimeSpan sunrise, TimeSpan sunset)
        {
            _sunrise = sunrise;
            _sunset = sunset;
        }
    }
}
