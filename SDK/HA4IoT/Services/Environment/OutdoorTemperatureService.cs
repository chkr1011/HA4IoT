using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Environment
{
    [ApiServiceClass(typeof(IOutdoorTemperatureService))]
    public class OutdoorTemperatureService : ServiceBase, IOutdoorTemperatureService
    {
        private readonly IDateTimeService _dateTimeService;

        public OutdoorTemperatureService(IDateTimeService dateTimeService, IApiService apiService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;

            apiService.StatusRequested += (s, e) =>
            {
                e.Context.Response.Merge(JObject.FromObject(this));
            };
        }

        public float OutdoorTemperature { get; private set; }

        [JsonIgnore]
        public DateTime? Timestamp { get; private set; }

        public void Update(float outdoorTemperature)
        {
            // TODO: Check for significant changes and round value.
            OutdoorTemperature = (float)Math.Round(Convert.ToDouble(outdoorTemperature), 1);
            Timestamp = _dateTimeService.Now;
        }

        [ApiMethod]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = JObject.FromObject(this);
        }
    }
}
