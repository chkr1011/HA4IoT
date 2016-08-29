using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Json;

namespace HA4IoT.Services.Environment
{
    [ApiServiceClass(typeof(IOutdoorHumidityService))]
    public class OutdoorHumidityService : ServiceBase, IOutdoorHumidityService
    {
        private readonly IDateTimeService _dateTimeService;

        public OutdoorHumidityService(IDateTimeService dateTimeService, IApiService apiService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;

            apiService.StatusRequested += (s, e) =>
            {
                e.Context.Response.Import(this.ToJsonObject(ToJsonObjectMode.Explicit));
            };
        }

        [JsonMember]
        public float OutdoorHumidity { get; private set; }

        public DateTime? Timestamp { get; private set; }

        [ApiMethod(ApiCallType.Request)]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = this.ToJsonObject();
        }

        public void Update(float outdoorHumidity)
        {
            // TODO: Check for significant changes and round value.
            OutdoorHumidity = outdoorHumidity;
            Timestamp = _dateTimeService.Now;
        }
    }
}
