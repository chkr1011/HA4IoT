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

        public OutdoorTemperatureService(IDateTimeService dateTimeService, IApiDispatcherService apiService)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));

            apiService.StatusRequested += (s, e) =>
            {
                e.ApiContext.Result.Merge(JObject.FromObject(this));
            };
        }

        public float OutdoorTemperature { get; private set; }

        [JsonIgnore]
        public DateTime? Timestamp { get; private set; }

        [ApiMethod]
        public void GetStatus(IApiCall apiCall)
        {
            apiCall.Result = JObject.FromObject(this);
        }

        public void Update(float value)
        {
            OutdoorTemperature = (float)Math.Round(Convert.ToDouble(value), 1);
            Timestamp = _dateTimeService.Now;
        }
    }
}
