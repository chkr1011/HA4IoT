using System;
using System.Diagnostics;
using System.IO;
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
using HA4IoT.Networking.Json;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    [ApiServiceClass(typeof(OpenWeatherMapService))]
    public class OpenWeatherMapService : ServiceBase
    {
        private readonly string _cacheFilename = StoragePath.WithFilename("OpenWeatherMapCache.json");

        private readonly IDateTimeService _dateTimeService;
        private readonly ISystemInformationService _systemInformationService;
        
        private string _previousResponse;

        [JsonMember]
        public float Temperature { get; private set; }
        [JsonMember]
        public float Humidity { get; private set; }
        [JsonMember]
        public TimeSpan Sunrise { get; private set; }
        [JsonMember]
        public TimeSpan Sunset { get; private set; }
        [JsonMember]
        public Weather Weather { get; private set; }
        
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

        [ApiMethod(ApiCallType.Command)]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response = this.ToJsonObject(ToJsonObjectMode.Explicit);
        }

        [ApiMethod(ApiCallType.Command)]
        public void Refresh(IApiContext apiContext)
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

                Weather = parser.Weather;
                WeatherFetched?.Invoke(this, new WeatherFetchedEventArgs(parser.Weather));
                
                Temperature = parser.Temperature;
                OutdoorTemperatureFetched?.Invoke(this, new OutdoorTemperatureFetchedEventArgs(Temperature));

                Humidity = parser.Humidity;
                OutdoorHumidityFetched?.Invoke(this, new OutdoorHumidityFetchedEventArgs(Humidity));

                Sunrise = parser.Sunrise;
                Sunset = parser.Sunset;
                DaylightFetched?.Invoke(this, new DaylightFetchedEventArgs(Sunrise, Sunset));
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