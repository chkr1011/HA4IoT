using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;

namespace HA4IoT.Services.Environment
{
    public class DaylightService : ServiceBase, IDaylightService
    {
        private readonly IDateTimeService _dateTimeService;

        public DaylightService(IDaylightProvider provider, IDateTimeService dateTimeService)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;
            provider.DaylightFetched += Update;
        }

        public TimeSpan Sunrise { get; private set; }
        public TimeSpan Sunset { get; private set; }
        public DateTime? Timestamp { get; private set; }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedTimeSpan("Sunrise", Sunrise);
            status.SetNamedTimeSpan("Sunset", Sunset);
            status.SetNamedDateTime("Timestamp", Timestamp);

            return status;
        }

        private void Update(object sender, DaylightFetchedEventArgs e)
        {
            Sunrise = e.Sunrise;
            Sunset = e.Sunset;
            Timestamp = _dateTimeService.Now;
        }
    }
}
