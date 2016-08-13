using System;
using System.Diagnostics;
using System.IO;
using Windows.Data.Json;
using Windows.Web.Http;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Networking;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapService : ServiceBase, IOutdoorTemperatureProvider, IOutdoorHumidityProvider, IDaylightProvider, IWeatherProvider
    {
        private readonly string _cacheFilename = StoragePath.WithFilename("OpenWeatherMapCache.json");

        private readonly IDateTimeService _dateTimeService;
        private readonly ISystemInformationService _systemInformationService;
        
        private string _previousResponse;

        private float _temperature;
        private float _humidity;
        private TimeSpan _sunrise;
        private TimeSpan _sunset;
        private Weather _weather;
        
        public OpenWeatherMapService(
            IDateTimeService dateTimeService, 
            ISchedulerService schedulerService, 
            ISystemInformationService systemInformationService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            _dateTimeService = dateTimeService;
            _systemInformationService = systemInformationService;
            
            LoadPersistedValues();
            
            schedulerService.RegisterSchedule("OpenWeatherMapServiceUpdater", TimeSpan.FromMinutes(5), Refresh);
        }

        public event EventHandler<OutdoorTemperatureFetchedEventArgs> OutdoorTemperatureFetched;
        public event EventHandler<OutdoorHumidityFetchedEventArgs> OutdoorHumidityFetched;
        public event EventHandler<DaylightFetchedEventArgs> DaylightFetched;
        public event EventHandler<WeatherFetchedEventArgs> WeatherFetched;

        public override JsonObject ExportStatusToJsonObject()
        {
            var result = new JsonObject();

            result.SetNamedString("situation", _weather.ToString());
            result.SetNamedNumber("temperature", _temperature);
            result.SetNamedNumber("humidity", _humidity);
            
            result.SetNamedTimeSpan("sunrise", _sunrise);
            result.SetNamedTimeSpan("sunset", _sunset);

            return result;
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

                _systemInformationService.Set("OpenWeatherMapService/LastUpdatedTimestamp", _dateTimeService.Now);
            }

            _systemInformationService.Set("OpenWeatherMapService/LastFetchedTimestamp", _dateTimeService.Now);
        }

        private string FetchWeatherData()
        {
            Uri uri = new OpenWeatherMapConfigurationParser().GetUri();

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

        private void ParseWeatherData(string weatherData)
        {
            try
            {
                var parser = new OpenWeatherMapResponseParser();
                parser.Parse(weatherData);

                _weather = parser.Weather;
                WeatherFetched?.Invoke(this, new WeatherFetchedEventArgs(parser.Weather));
                
                _temperature = parser.Temperature;
                OutdoorTemperatureFetched?.Invoke(this, new OutdoorTemperatureFetchedEventArgs(_temperature));

                _humidity = parser.Humidity;
                OutdoorHumidityFetched?.Invoke(this, new OutdoorHumidityFetchedEventArgs(_humidity));

                _sunrise = parser.Sunrise;
                _sunset = parser.Sunset;
                DaylightFetched?.Invoke(this, new DaylightFetchedEventArgs(_sunrise, _sunset));
            }
            catch (Exception exception)
            {
                Log.Warning(exception, $"Error while parsing Open Weather Map response ({weatherData}).");
            }
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