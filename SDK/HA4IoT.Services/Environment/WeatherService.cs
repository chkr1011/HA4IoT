using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Networking;

namespace HA4IoT.Services.Environment
{
    public class WeatherService : ServiceBase, IWeatherService
    {
        private readonly IDateTimeService _dateTimeService;

        public WeatherService(IWeatherProvider provider, IDateTimeService dateTimeService)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;
            provider.WeatherFetched += Update;
        }

        public DateTime? Timestamp { get; private set; }

        public Weather Weather { get; private set; }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedEnum("Weather", Weather);
            status.SetNamedDateTime("Timestamp", Timestamp);

            return status;
        }

        private void Update(object sender, WeatherFetchedEventArgs e)
        {
            Weather = e.Weather;
            Timestamp = _dateTimeService.Now;
        }
    }
}
