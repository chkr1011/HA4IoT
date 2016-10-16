using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Environment
{
    [ApiServiceClass(typeof(IWeatherService))]
    public class WeatherService : ServiceBase, IWeatherService
    {
        private readonly IDateTimeService _dateTimeService;

        public WeatherService(IDateTimeService dateTimeService, IApiService apiService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;

            apiService.StatusRequested += (s, e) =>
            {
                e.Context.Response.Merge(JObject.FromObject(this));
            };
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public Weather Weather { get; private set; }

        public DateTime? Timestamp { get; private set; }
        
        [ApiMethod]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = JObject.FromObject(this);
        }

        public void Update(Weather weather)
        {
            Weather = weather;
            Timestamp = _dateTimeService.Now;
        }
    }
}
