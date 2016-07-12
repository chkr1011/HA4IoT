using System;
using System.Diagnostics;
using System.IO;
using Windows.Data.Json;
using Windows.Web.Http;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Networking;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapService : ServiceBase
    {
        private readonly string _cacheFilename = StoragePath.WithFilename("OpenWeatherMapCache.json");

        private readonly IDateTimeService _dateTimeService;
        private readonly ISystemInformationService _systemInformationService;

        private readonly OpenWeatherMapOutdoorTemperatureService _outdoorTemperatureService = new OpenWeatherMapOutdoorTemperatureService();
        private readonly OpenWeatherMapOutdoorHumidityService _outdoorHumdityService = new OpenWeatherMapOutdoorHumidityService();
        private readonly OpenWeatherMapWeatherService _weatherService = new OpenWeatherMapWeatherService();
        private readonly OpenWeatherMapDaylightService _daylightService = new OpenWeatherMapDaylightService();
        
        private string _previousResponse;
        
        public OpenWeatherMapService(IApiController apiController, IDateTimeService dateTimeService, ISchedulerService schedulerService, ISystemInformationService systemInformationService)
        {
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            _dateTimeService = dateTimeService;
            _systemInformationService = systemInformationService;
            
            LoadPersistedValues();

            new OpenWeatherMapWeatherStationApiDispatcher(this, apiController).ExposeToApi();

            schedulerService.RegisterSchedule("OpenWeatherMapServiceUpdater", TimeSpan.FromMinutes(5), Refresh);
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var result = new JsonObject();

            result.SetNamedString("situation", _weatherService.GetWeather().ToString());
            result.SetNamedNumber("temperature", _outdoorTemperatureService.GetOutdoorTemperature());
            result.SetNamedNumber("humidity", _outdoorHumdityService.GetOutdoorHumidity());
            
            result.SetNamedTimeSpan("sunrise", _daylightService.GetSunrise());
            result.SetNamedTimeSpan("sunset", _daylightService.GetSunset());

            return result;
        }
        
        public void SetStatus(WeatherSituation situation, float temperature, float humidity, TimeSpan sunrise, TimeSpan sunset)
        {
            _weatherService.Update(situation);
            _outdoorTemperatureService.Update(temperature);
            _outdoorHumdityService.Update(humidity);
            _daylightService.Update(sunrise, sunset);
        }

        public override void CompleteRegistration(IServiceLocator serviceLocator)
        {
            serviceLocator.RegisterService(typeof(IOutdoorTemperatureService), _outdoorTemperatureService);
            serviceLocator.RegisterService(typeof(IOutdoorHumidityService), _outdoorHumdityService);
            serviceLocator.RegisterService(typeof(IWeatherService), _weatherService);
            serviceLocator.RegisterService(typeof(IDaylightService), _daylightService);
        }

        public override void HandleApiCommand(IApiContext apiContext)
        {
            Refresh();
        }

        private void PersistWeatherData(string weatherData)
        {
            File.WriteAllText(_cacheFilename, weatherData);
        }

        private void Refresh()
        {
            Log.Verbose("Fetching OWM weather data");
            string response = FetchWeatherData();

            if (!string.Equals(response, _previousResponse))
            {
                PersistWeatherData(response);
                ParseWeatherData(response);

                _previousResponse = response;

                _systemInformationService.Set("OpenWeatherMapService/LastUpdatedTimestamp", _dateTimeService.GetDateTime());
            }

            _systemInformationService.Set("OpenWeatherMapService/LastFetchedTimestamp", _dateTimeService.GetDateTime());
        }

        private string FetchWeatherData()
        {
            Uri uri = new OpenWeatherMapConfigurationParser().GetUri();

            _systemInformationService.Set("OpenWeatherMapService/Uri", uri.ToString());

            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var httpClient = new HttpClient())
                using (HttpResponseMessage result = httpClient.GetAsync(uri).GetResults())
                {
                    return result.Content.ReadAsStringAsync().GetResults();
                }
            }
            finally
            {
                _systemInformationService.Set("OpenWeatherMapService/LastFetchDuration", stopwatch.Elapsed);
            }
        }

        private void ParseWeatherData(string weatherData)
        {
            var parser = new OpenWeatherMapResponseParser();
            parser.Parse(weatherData);

            _weatherService.Update(parser.Situation);
            _outdoorTemperatureService.Update(parser.Temperature);
            _outdoorHumdityService.Update(parser.Humidity);
            _daylightService.Update(parser.Sunrise, parser.Sunset);
        }

        private void LoadPersistedValues()
        {
            if (!File.Exists(_cacheFilename))
            {
                return;
            }

            try
            {
                ParseWeatherData(File.ReadAllText(_cacheFilename));
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unable to load cached weather data.");
                File.Delete(_cacheFilename);
            }
        }
    }
}