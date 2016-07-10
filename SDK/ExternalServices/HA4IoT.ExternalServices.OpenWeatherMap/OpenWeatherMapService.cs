using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        
        public OpenWeatherMapService(IApiController apiController, IDateTimeService dateTimeService, ISystemInformationService systemInformationService)
        {
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            _dateTimeService = dateTimeService;
            _systemInformationService = systemInformationService;
            
            LoadPersistedValues();

            Task.Factory.StartNew(
                async () => await FetchWeahterData(),
                CancellationToken.None,
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Default);

            new OpenWeatherMapWeatherStationApiDispatcher(this, apiController).ExposeToApi();
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
            serviceLocator.RegisterService(_outdoorTemperatureService);
            serviceLocator.RegisterService(_outdoorHumdityService);
            serviceLocator.RegisterService(_weatherService);
            serviceLocator.RegisterService(_daylightService);
        }

        private void PersistWeatherData(string weatherData)
        {
            File.WriteAllText(_cacheFilename, weatherData);
        }

        private async Task FetchWeahterData()
        {
            while (true)
            {
                try
                {
                    Log.Verbose("Fetching OWM weather data");
                    string response = await FetchWeatherData();

                    if (!string.Equals(response, _previousResponse))
                    {
                        PersistWeatherData(response);
                        ParseWeatherData(response);

                        _previousResponse = response;

                        _systemInformationService.Set("OpenWeatherMapService/LastUpdatedTimestamp", _dateTimeService.GetDateTime());
                    }

                    _systemInformationService.Set("OpenWeatherMapService/LastFetchedTimestamp", _dateTimeService.GetDateTime());
                }
                catch (Exception exception)
                {
                    Log.Warning(exception, "Could not fetch OWM weather data");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }
        }

        private async Task<string> FetchWeatherData()
        {
            Uri uri = new OpenWeatherMapConfigurationParser().GetUri();

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