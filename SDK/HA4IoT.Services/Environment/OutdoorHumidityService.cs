using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;

namespace HA4IoT.Services.Environment
{
    public class OutdoorHumidityService : ServiceBase, IOutdoorHumidityService
    {
        private readonly IDateTimeService _dateTimeService;

        public OutdoorHumidityService(IOutdoorHumidityProvider provider, IDateTimeService dateTimeService)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;
            provider.OutdoorHumidityFetched += Update;
        }

        public float OutdoorHumidity { get; private set; }

        public DateTime? Timestamp { get; private set; }

        private void Update(object sender, OutdoorHumidityFetchedEventArgs e)
        {
            // TODO: Check for significant changes and round value.
            OutdoorHumidity = e.OutdoorHumidity;
            Timestamp = _dateTimeService.Now;
        }

        public override JsonObject GetStatus()
        {
            var status = base.GetStatus();
            status.SetNamedNumber("OutdoorHumidity", OutdoorHumidity);
            status.SetNamedDateTime("Timestamp", Timestamp);

            return status;
        }
    }
}
