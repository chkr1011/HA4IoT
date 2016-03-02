using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Web.Http;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class OpenWeatherMapWeatherStation : IWeatherStation
    {
        private readonly string _cacheFilename = Path.Combine(ApplicationData.Current.LocalFolder.Path,
            "OWMCache.json");

        public static readonly DeviceId DefaultDeviceId = new DeviceId("OWMWeatherStation");

        private readonly IHomeAutomationTimer _timer;
        private readonly ILogger _logger;

        private readonly WeatherStationTemperatureSensor _temperature;
        private readonly WeatherStationHumiditySensor _humidity;
        private readonly WeatherStationSituationSensor _situation;

        private string _previousResponse;
        private DateTime? _lastFetched;
        private DateTime? _lastFetchedDifferentResponse;
        private TimeSpan _sunrise;
        private TimeSpan _sunset;
        
        public OpenWeatherMapWeatherStation(DeviceId id, IHomeAutomationTimer timer, IHttpRequestController httpApiController, ILogger logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _temperature = new WeatherStationTemperatureSensor(new ActuatorId("WeatherStation.Temperature"), httpApiController, logger);
            TemperatureSensor = _temperature;

            _humidity = new WeatherStationHumiditySensor(new ActuatorId("WeatherStation.Humidity"), httpApiController, logger);
            HumiditySensor = _humidity;

            _situation = new WeatherStationSituationSensor(new ActuatorId("WeatherStation.Situation"), httpApiController, logger);
            SituationSensor = _situation;

            Id = id;
            _timer = timer;
            _logger = logger;
   
            LoadPersistedValues();

            Task.Factory.StartNew(async () => await FetchWeahterData(), CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);

            new OpenWeatherMapWeatherStationHttpApiDispatcher(this, httpApiController).ExposeToApi();
        }

        public DeviceId Id { get; }

        // TODO: Move Daylight to other service because it is not part of weather state.
        public Daylight Daylight => new Daylight(_timer.CurrentTime, _sunrise, _sunset);

        public IWeatherSituationSensor SituationSensor { get; }
        public ITemperatureSensor TemperatureSensor { get; }
        public IHumiditySensor HumiditySensor { get; }
        
        public JsonObject ExportStatusToJsonObject()
        {
            var result = new JsonObject();

            var configurationParser = new OpenWeatherMapConfigurationParser(_logger);
            result.SetNamedValue("uri", configurationParser.GetUri().ToString().ToJsonValue());

            result.SetNamedValue("situation", SituationSensor.GetSituation().ToJsonValue());
            result.SetNamedValue("temperature", TemperatureSensor.GetValue().ToJsonValue());
            result.SetNamedValue("humidity", HumiditySensor.GetValue().ToJsonValue());

            result.SetNamedValue("lastFetched", _lastFetched.ToJsonValue());
            result.SetNamedValue("lastFetchedDifferentResponse", _lastFetchedDifferentResponse.ToJsonValue());

            result.SetNamedValue("sunrise", _sunrise.ToJsonValue());
            result.SetNamedValue("sunset", _sunset.ToJsonValue());

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
                    _logger.Verbose("Fetching OWM weather data");
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
                    _logger.Warning(exception, "Could not fetch OWM weather data");
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
            Uri uri = new OpenWeatherMapConfigurationParser(_logger).GetUri();

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
                _logger.Warning(exception, "Unable to load cached weather data.");
                File.Delete(_cacheFilename);
            }
        }
    }
}