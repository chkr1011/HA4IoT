using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.WeatherService;
using HA4IoT.Networking;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapWeatherService : ServiceBase, IWeatherService, IDaylightService
    {
        private readonly string _cacheFilename = StoragePath.WithFilename("OpenWeatherMapCache.json");

        private readonly IHomeAutomationTimer _timer;

        private float _temperature;
        private float _humidity;
        private WeatherSituation _situation;

        private TimeSpan _sunrise;
        private TimeSpan _sunset;
        private string _previousResponse;
        private DateTime? _lastFetched;
        private DateTime? _lastFetchedDifferentResponse;
        
        public OpenWeatherMapWeatherService(IHomeAutomationTimer timer, IApiController apiController)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));
            
            _timer = timer;
    
            LoadPersistedValues();

            Task.Factory.StartNew(
                async () => await FetchWeahterData(),
                CancellationToken.None,
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Default);

            new OpenWeatherMapWeatherStationApiDispatcher(this, apiController).ExposeToApi();
        }

        public float GetTemperature()
        {
            return _temperature;
        }

        public float GetHumidity()
        {
            return _humidity;
        }

        public WeatherSituation GetSituation()
        {
            return _situation;
        }

        public TimeSpan GetSunrise()
        {
            return _sunrise;
        }

        public TimeSpan GetSunset()
        {
            return _sunset;
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var result = new JsonObject();

            result.SetNamedString("situation", _situation.ToString());
            result.SetNamedNumber("temperature", _temperature);
            result.SetNamedNumber("humidity", _humidity);

            result.SetNamedDateTime("lastFetched", _lastFetched);
            result.SetNamedDateTime("lastFetchedDifferentResponse", _lastFetchedDifferentResponse);

            result.SetNamedTimeSpan("sunrise", _sunrise);
            result.SetNamedTimeSpan("sunset", _sunset);

            return result;
        }
        
        public override void HandleApiRequest(IApiContext apiContext)
        {
            var configurationParser = new OpenWeatherMapConfigurationParser();

            apiContext.Response = ExportStatusToJsonObject();
            apiContext.Response.SetNamedString("Uri", configurationParser.GetUri().ToString());
        }

        public void SetStatus(WeatherSituation situation, float temperature, float humidity, TimeSpan sunrise, TimeSpan sunset)
        {
            _situation = situation;
            _temperature = temperature;
            _humidity = humidity;

            _sunrise = sunrise;
            _sunset = sunset;
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
                        _lastFetchedDifferentResponse = _timer.CurrentDateTime;
                    }

                    _lastFetched = _timer.CurrentDateTime;
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

            using (var httpClient = new HttpClient())
            using (HttpResponseMessage result = await httpClient.GetAsync(uri))
            {
                return await result.Content.ReadAsStringAsync();
            }
        }

        private void ParseWeatherData(string weatherData)
        {
            var parser = new OpenWeatherMapResponseParser();
            parser.Parse(weatherData);

            _situation = parser.Situation;
            _temperature = parser.Temperature;
            _humidity = parser.Humidity;

            _sunrise = parser.Sunrise;
            _sunset = parser.Sunset;
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