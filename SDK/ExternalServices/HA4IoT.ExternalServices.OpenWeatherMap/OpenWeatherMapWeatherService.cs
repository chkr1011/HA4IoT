using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Web.Http;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services.WeatherService;
using HA4IoT.Networking;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapWeatherService : IWeatherService
    {
        private readonly string _cacheFilename = Path.Combine(ApplicationData.Current.LocalFolder.Path,
            "OpenWeatherMapCache.json");

        private readonly IHomeAutomationTimer _timer;

        private readonly WeatherStationTemperatureSensor _temperature;
        private readonly WeatherStationHumiditySensor _humidity;
        private readonly WeatherStationSituationSensor _situation;

        private string _previousResponse;
        private DateTime? _lastFetched;
        private DateTime? _lastFetchedDifferentResponse;
        private TimeSpan _sunrise;
        private TimeSpan _sunset;
        
        public OpenWeatherMapWeatherService(IHomeAutomationTimer timer, IApiController apiController)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _temperature = new WeatherStationTemperatureSensor(new ActuatorId("WeatherStation.Temperature"), apiController);
            TemperatureSensor = _temperature;

            _humidity = new WeatherStationHumiditySensor(new ActuatorId("WeatherStation.Humidity"), apiController);
            HumiditySensor = _humidity;

            _situation = new WeatherStationSituationSensor(new ActuatorId("WeatherStation.Situation"), apiController);
            SituationSensor = _situation;

            _timer = timer;
    
            LoadPersistedValues();

            Task.Factory.StartNew(
                async () => await FetchWeahterData(),
                CancellationToken.None,
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Default);

            new OpenWeatherMapWeatherStationApiDispatcher(this, apiController).ExposeToApi();
        }

        // TODO: Move Daylight to other service because it is not part of weather state.
        public Daylight Daylight => new Daylight(_timer.CurrentTime, _sunrise, _sunset);

        public IWeatherSituationSensor SituationSensor { get; }
        public ITemperatureSensor TemperatureSensor { get; }
        public IHumiditySensor HumiditySensor { get; }
        
        public JsonObject ExportStatusToJsonObject()
        {
            var result = new JsonObject();

            var configurationParser = new OpenWeatherMapConfigurationParser();
            result.SetNamedString("uri", configurationParser.GetUri().ToString());

            result.SetNamedValue("situation", SituationSensor.GetSituation().ToJsonValue());
            result.SetNamedNumber("temperature", TemperatureSensor.GetValue());
            result.SetNamedNumber("humidity", HumiditySensor.GetValue());

            result.SetNamedDateTime("lastFetched", _lastFetched);
            result.SetNamedDateTime("lastFetchedDifferentResponse", _lastFetchedDifferentResponse);

            result.SetNamedTimeSpan("sunrise", _sunrise);
            result.SetNamedTimeSpan("sunset", _sunset);

            return result;
        }

        public void SetStatus(WeatherSituation situation, float temperature, float humidity, TimeSpan sunrise, TimeSpan sunset)
        {
            _situation.SetValue(situation);
            _temperature.SetValue(temperature);
            _humidity.SetValue(humidity);

            _sunrise = sunrise;
            _sunset = sunset;
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

        private void PersistWeatherData(string weatherData)
        {
            File.WriteAllText(_cacheFilename, weatherData);
        }

        private void ParseWeatherData(string weatherData)
        {
            var parser = new OpenWeatherMapResponseParser();
            parser.Parse(weatherData);

            _situation.SetValue(parser.Situation);
            _temperature.SetValue(parser.Temperature);
            _humidity.SetValue(parser.Humidity);

            _sunrise = parser.Sunrise;
            _sunset = parser.Sunset;
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