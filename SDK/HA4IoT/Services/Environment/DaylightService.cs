using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Environment
{
    [ApiServiceClass(typeof(IDaylightService))]
    public class DaylightService : ServiceBase, IDaylightService
    {
        private readonly IDateTimeService _dateTimeService;

        public DaylightService(IDateTimeService dateTimeService, IApiDispatcherService apiService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;

            apiService.StatusRequested += (s, e) =>
            {
                e.ApiContext.Result.Merge(JObject.FromObject(this));
            };
        }

        public TimeSpan Sunrise { get; private set; } = TimeSpan.Parse("06:45");
        public TimeSpan Sunset { get; private set; } = TimeSpan.Parse("20:30");

        [JsonIgnore]
        public DateTime? Timestamp { get; private set; }

        [ApiMethod]
        public void Status(IApiContext apiContext)
        {
            apiContext.Result = JObject.FromObject(this);
        }

        public void Update(TimeSpan sunrise, TimeSpan sunset)
        {
            Sunrise = sunrise;
            Sunset = sunset;
            Timestamp = _dateTimeService.Now;
        }
    }
}
