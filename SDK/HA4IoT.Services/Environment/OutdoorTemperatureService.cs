using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;
using HA4IoT.Networking.Json;

namespace HA4IoT.Services.Environment
{
    [ApiServiceClass(typeof(IOutdoorTemperatureService))]
    public class OutdoorTemperatureService : ServiceBase, IOutdoorTemperatureService
    {
        private readonly IDateTimeService _dateTimeService;

        public OutdoorTemperatureService(IOutdoorTemperatureProvider provider, IDateTimeService dateTimeService, IApiService apiService)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;

            provider.OutdoorTemperatureFetched += Update;

            apiService.StatusRequested += (s, e) =>
            {
                e.Context.Response.Import(this.ToJsonObject(ToJsonObjectMode.Explicit));
            };
        }

        [JsonMember]
        public float OutdoorTemperature { get; private set; }

        public DateTime? Timestamp { get; private set; }

        private void Update(object sender, OutdoorTemperatureFetchedEventArgs e)
        {
            // TODO: Check for significant changes and round value.
            OutdoorTemperature = (float)Math.Round(Convert.ToDouble(e.OutdoorTemperature), 1);
            Timestamp = _dateTimeService.Now;
        }

        [ApiMethod(ApiCallType.Request)]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = this.ToJsonObject();
        }
    }
}
