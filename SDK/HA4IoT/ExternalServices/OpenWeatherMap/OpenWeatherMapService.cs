using System;
using System.Diagnostics;
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

        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IOutdoorHumidityService _outdoorHumidityService;
        private readonly IDaylightService _daylightService;
        private readonly IWeatherService _weatherService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISystemInformationService _systemInformationService;
        private readonly IStorageService _storageService;

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
            IStorageService storageService)
        {
            if (outdoorTemperatureService == null) throw new ArgumentNullException(nameof(outdoorTemperatureService));
            if (outdoorHumidityService == null) throw new ArgumentNullException(nameof(outdoorHumidityService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (weatherService == null) throw new ArgumentNullException(nameof(weatherService));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (storageService == null) throw new ArgumentNullException(nameof(storageService));
            
            _outdoorTemperatureService = outdoorTemperatureService;
            _outdoorHumidityService = outdoorHumidityService;
            _daylightService = daylightService;
            _weatherService = weatherService;
            _dateTimeService = dateTimeService;
            _systemInformationService = systemInformationService;
            _storageService = storageService;

            settingsService.CreateSettingsMonitor<OpenWeatherMapServiceSettings>(s => Settings = s);

            LoadPersistedData();
            
            schedulerService.RegisterSchedule("OpenWeatherMapServiceUpdater", TimeSpan.FromMinutes(5), Refresh);
        }

        public OpenWeatherMapServiceSettings Settings { get; private set; }

        [ApiMethod]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = JObject.FromObject(this);
        }

        [ApiMethod]
        public void Refresh(IApiContext apiContext)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (!Settings.IsEnabled)
            {
                Log.Verbose("Fetching Open Weather Map Service is disabled.");
                return;
            }

            Log.Verbose("Fetching Open Weather Map weather data.");

            var response = FetchWeatherData();

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

        private string FetchWeatherData()
        {
            var uri = new Uri($"http://api.openweathermap.org/data/2.5/weather?lat={Settings.Latitude}&lon={Settings.Longitude}&APPID={Settings.AppId}&units=metric");

            _systemInformationService.Set("OpenWeatherMapService/Uri", uri.ToString());

            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var httpClient = new HttpClient())
                using (HttpResponseMessage result = httpClient.GetAsync(uri).AsTask().Result)
                {
                    return result.Content.ReadAsStringAsync().AsTask().Result;
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
                Log.Warning(exception, $"Error while parsing Open Weather Map response ({weatherData}).");

                return false;
            }
        }

        private void LoadPersistedData()
        {
            JObject cachedResponse;
            if (_storageService.TryRead(StorageFilename, out cachedResponse))
            {
                TryParseData(cachedResponse.ToString());
            }
        }
    }
}