using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Web.Http;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.Storage;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    [ApiServiceClass(typeof(OpenWeatherMapService))]
    public class OpenWeatherMapService : ServiceBase
    {
        private const string StorageFilename = "OpenWeatherMapCache.json";

        private readonly ISchedulerService _schedulerService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IOutdoorHumidityService _outdoorHumidityService;
        private readonly IDaylightService _daylightService;
        private readonly IWeatherService _weatherService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISystemInformationService _systemInformationService;
        private readonly IStorageService _storageService;
        private readonly ILogger _log;

        private string _previousResponse;

        public float Temperature { get; private set; }
        public float Humidity { get; private set; }
        public TimeSpan Sunrise { get; private set; }
        public TimeSpan Sunset { get; private set; }
        public Weather Weather { get; private set; }
        
        public OpenWeatherMapService(
            IOutdoorTemperatureService outdoorTemperatureService,
            IOutdoorHumidityService outdoorHumidityService,
            IDaylightService daylightService,
            IWeatherService weatherService,
            IDateTimeService dateTimeService, 
            ISchedulerService schedulerService, 
            ISystemInformationService systemInformationService,
            ISettingsService settingsService, 
            IStorageService storageService,
            ILogService logService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _outdoorTemperatureService = outdoorTemperatureService ?? throw new ArgumentNullException(nameof(outdoorTemperatureService));
            _outdoorHumidityService = outdoorHumidityService ?? throw new ArgumentNullException(nameof(outdoorHumidityService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _systemInformationService = systemInformationService ?? throw new ArgumentNullException(nameof(systemInformationService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

            _log = logService?.CreatePublisher(nameof(OpenWeatherMapService)) ?? throw new ArgumentNullException(nameof(logService));

            settingsService.CreateSettingsMonitor<OpenWeatherMapServiceSettings>(s => Settings = s.NewSettings);           
        }

        public OpenWeatherMapServiceSettings Settings { get; private set; }

        public override void Startup()
        {
            if (!Settings.IsEnabled)
            {
                _log.Info("Open Weather Map Service is disabled.");
                return;
            }

            LoadPersistedData();
            _schedulerService.RegisterSchedule("OpenWeatherMapServiceUpdater", TimeSpan.FromMinutes(5), RefreshAsync);
        }

        [ApiMethod]
        public void Status(IApiContext apiContext)
        {
            apiContext.Result = JObject.FromObject(this);
        }

        [ApiMethod]
        public void Refresh(IApiContext apiContext)
        {
            RefreshAsync().Wait();
        }

        private async Task RefreshAsync()
        {
            if (!Settings.IsEnabled)
            {
                _log.Verbose("Fetching Open Weather Map Service is disabled.");
                return;
            }

            _log.Verbose("Fetching Open Weather Map weather data.");

            var response = await FetchWeatherDataAsync();

            if (!string.Equals(response, _previousResponse))
            {
                if (TryParseData(response))
                {
                    _storageService.Write(StorageFilename, response);
                    PushData();
                }
                
                _previousResponse = response;

                _systemInformationService.Set("OpenWeatherMapService/LastUpdatedTimestamp", _dateTimeService.Now);
            }

            _systemInformationService.Set("OpenWeatherMapService/LastFetchedTimestamp", _dateTimeService.Now);
        }

        private void PushData()
        {
            if (Settings.UseTemperature)
            {
                _outdoorTemperatureService.Update(Temperature);
            }

            if (Settings.UseHumidity)
            {
                _outdoorHumidityService.Update(Humidity);
            }

            if (Settings.UseSunriseSunset)
            {
                _daylightService.Update(Sunrise, Sunset);
            }

            if (Settings.UseWeather)
            {
                _weatherService.Update(Weather);
            }
        }

        private async Task<string> FetchWeatherDataAsync()
        {
            var uri = new Uri($"http://api.openweathermap.org/data/2.5/weather?lat={Settings.Latitude}&lon={Settings.Longitude}&APPID={Settings.AppId}&units=metric");

            _systemInformationService.Set("OpenWeatherMapService/Uri", uri.ToString());

            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var httpClient = new HttpClient())
                using (HttpResponseMessage result = await httpClient.GetAsync(uri))
                {
                    return await result.Content.ReadAsStringAsync();
                }
            }
            finally
            {
                _systemInformationService.Set("OpenWeatherMapService/LastFetchDuration", stopwatch.Elapsed);
            }
        }

        private bool TryParseData(string weatherData)
        {
            try
            {
                var parser = new OpenWeatherMapResponseParser();
                parser.Parse(weatherData);

                Weather = parser.Weather;
                Temperature = parser.Temperature;
                Humidity = parser.Humidity;

                Sunrise = parser.Sunrise;
                Sunset = parser.Sunset;

                return true;
            }
            catch (Exception exception)
            {
                _log.Warning(exception, $"Error while parsing Open Weather Map response ({weatherData}).");

                return false;
            }
        }

        private void LoadPersistedData()
        {
            string cachedResponse;
            if (_storageService.TryRead(StorageFilename, out cachedResponse))
            {
                TryParseData(cachedResponse);
            }
        }
    }
}