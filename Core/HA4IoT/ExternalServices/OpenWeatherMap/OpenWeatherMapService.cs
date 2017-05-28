using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Web.Http;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Environment;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Settings;
using HA4IoT.Contracts.Storage;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    [ApiServiceClass(typeof(OpenWeatherMapService))]
    public class OpenWeatherMapService : ServiceBase
    {
        private const string StorageFilename = "OpenWeatherMapCache.json";

        private readonly IOutdoorService _outdoorService;
        private readonly IDaylightService _daylightService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISystemInformationService _systemInformationService;
        private readonly IStorageService _storageService;
        private readonly ILogger _log;

        private string _previousResponse;

        public float Temperature { get; private set; }
        public float Humidity { get; private set; }
        public TimeSpan Sunrise { get; private set; }
        public TimeSpan Sunset { get; private set; }
        public WeatherCondition Condition { get; private set; }
        
        public OpenWeatherMapService(
            IOutdoorService outdoorService,
            IDaylightService daylightService,
            IDateTimeService dateTimeService, 
            ISchedulerService schedulerService, 
            ISystemInformationService systemInformationService,
            ISettingsService settingsService, 
            IStorageService storageService,
            ILogService logService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            _outdoorService = outdoorService ?? throw new ArgumentNullException(nameof(outdoorService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _systemInformationService = systemInformationService ?? throw new ArgumentNullException(nameof(systemInformationService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

            _log = logService?.CreatePublisher(nameof(OpenWeatherMapService)) ?? throw new ArgumentNullException(nameof(logService));

            settingsService.CreateSettingsMonitor<OpenWeatherMapServiceSettings>(s => Settings = s.NewSettings);

            LoadPersistedData();
            schedulerService.Register("OpenWeatherMapServiceUpdater", TimeSpan.FromMinutes(5), RefreshAsync);
        }

        public OpenWeatherMapServiceSettings Settings { get; private set; }

        [ApiMethod]
        public void Status(IApiCall apiCall)
        {
            apiCall.Result = JObject.FromObject(this);
        }

        [ApiMethod]
        public void Refresh(IApiCall apiCall)
        {
            RefreshAsync().Wait();
        }

        private async Task RefreshAsync()
        {
            if (!Settings.IsEnabled)
            {
                _log.Verbose("OpenWeatherMapService is disabled.");
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
                _outdoorService.UpdateTemperature(Temperature);
            }

            if (Settings.UseHumidity)
            {
                _outdoorService.UpdateHumidity(Humidity);
            }

            if (Settings.UseSunriseSunset)
            {
                _daylightService.Update(Sunrise, Sunset);
            }

            if (Settings.UseWeather)
            {
                _outdoorService.UpdateCondition(Condition);
            }
        }

        private async Task<string> FetchWeatherDataAsync()
        {
            var uri = new Uri($"http://api.openweathermap.org/data/2.5/weather?lat={Settings.Latitude}&lon={Settings.Longitude}&APPID={Settings.AppId}&units=metric");

            _systemInformationService.Set($"{nameof(OpenWeatherMapService)}/Uri", uri.ToString());

            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var httpClient = new HttpClient())
                using (var result = await httpClient.GetAsync(uri))
                {
                    return await result.Content.ReadAsStringAsync();
                }
            }
            finally
            {
                _systemInformationService.Set($"{nameof(OpenWeatherMapService)}/LastFetchDuration", stopwatch.Elapsed);
            }
        }

        private bool TryParseData(string weatherData)
        {
            try
            {
                var parser = new OpenWeatherMapResponseParser();
                parser.Parse(weatherData);

                _systemInformationService.Set($"{nameof(OpenWeatherMapService)}/LastConditionCode", parser.ConditionCode);

                Condition = parser.Condition;
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