using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Networking.Json;

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
                e.Context.Response.Import(this.ToJsonObject(ToJsonObjectMode.Explicit));
            };
        }

        [JsonMember]
        public Weather Weather { get; private set; }

        public DateTime? Timestamp { get; private set; }
        
        [ApiMethod(ApiCallType.Request)]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = this.ToJsonObject();
        }

        public void Update(Weather weather)
        {
            Weather = weather;
            Timestamp = _dateTimeService.Now;
        }
    }
}
