using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;

namespace HA4IoT.Services.Environment
{
    public class OutdoorTemperatureService : ServiceBase, IOutdoorTemperatureService
    {
        private readonly IDateTimeService _dateTimeService;

        public OutdoorTemperatureService(IOutdoorTemperatureProvider provider, IDateTimeService dateTimeService)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;
            provider.OutdoorTemperatureFetched += Update;
        }

        public float OutdoorTemperature { get; private set; }

        public DateTime? Timestamp { get; private set; }

        private void Update(object sender, OutdoorTemperatureFetchedEventArgs e)
        {
            // TODO: Check for significant changes and round value.
            OutdoorTemperature = (float)Math.Round(Convert.ToDouble(e.OutdoorTemperature), 1);
            Timestamp = _dateTimeService.Now;
        }

        public override JsonObject GetStatus()
        {
            var status = base.GetStatus();
            status.SetNamedNumber("OutdoorTemperature", OutdoorTemperature);
            status.SetNamedDateTime("Timestamp", Timestamp);

            return status;
        }
    }
}
