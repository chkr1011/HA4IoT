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

        public WeatherService(IWeatherProvider provider, IDateTimeService dateTimeService, IApiService apiService)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;

            provider.WeatherFetched += Update;

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

        private void Update(object sender, WeatherFetchedEventArgs e)
        {
            Weather = e.Weather;
            Timestamp = _dateTimeService.Now;
        }
    }
}
