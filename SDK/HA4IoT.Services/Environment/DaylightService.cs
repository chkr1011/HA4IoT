using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Json;

namespace HA4IoT.Services.Environment
{
    [ApiServiceClass(typeof(IDaylightService))]
    public class DaylightService : ServiceBase, IDaylightService
    {
        private readonly IDateTimeService _dateTimeService;

        public DaylightService(IDaylightProvider provider, IDateTimeService dateTimeService, IApiService apiService)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;

            provider.DaylightFetched += Update;

            apiService.StatusRequested += (s, e) =>
            {
                e.Context.Response.Import(this.ToJsonObject(ToJsonObjectMode.Explicit));
            };
        }

        [JsonMember]
        public TimeSpan Sunrise { get; private set; }
        [JsonMember]
        public TimeSpan Sunset { get; private set; }
        public DateTime? Timestamp { get; private set; }

        [ApiMethod(ApiCallType.Request)]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = this.ToJsonObject();
        }

        private void Update(object sender, DaylightFetchedEventArgs e)
        {
            Sunrise = e.Sunrise;
            Sunset = e.Sunset;
            Timestamp = _dateTimeService.Now;
        }
    }
}
