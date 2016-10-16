using System;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Weather;
using System.Net.Http;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.ControllerSlave
{
    [ApiServiceClass(typeof(ControllerSlaveService))]
    public class ControllerSlaveService : ServiceBase
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IOutdoorHumidityService _outdoorHumidityService;
        private readonly IDaylightService _daylightService;
        private readonly IWeatherService _weatherService;

        private DateTime? _lastPull;
        private DateTime? _lastSuccessfulPull;

        public ControllerSlaveService(
            ISettingsService settingsService,
            ISchedulerService scheduler,
            IDateTimeService dateTimeService,
            IOutdoorTemperatureService outdoorTemperatureService,
            IOutdoorHumidityService outdoorHumidityService,
            IDaylightService daylightService,
            IWeatherService weatherService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (outdoorTemperatureService == null) throw new ArgumentNullException(nameof(outdoorTemperatureService));
            if (outdoorHumidityService == null) throw new ArgumentNullException(nameof(outdoorHumidityService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (weatherService == null) throw new ArgumentNullException(nameof(weatherService));

            _dateTimeService = dateTimeService;
            _outdoorTemperatureService = outdoorTemperatureService;
            _outdoorHumidityService = outdoorHumidityService;
            _daylightService = daylightService;
            _weatherService = weatherService;

            settingsService.CreateSettingsMonitor<ControllerSlaveServiceSettings>(s => Settings = s);

            scheduler.RegisterSchedule("ControllerSlavePolling", TimeSpan.FromMinutes(5), PullValues);
        }

        public ControllerSlaveServiceSettings Settings { get; private set; }

        [ApiMethod]
        public void Status(IApiContext apiContext)
        {
            apiContext.Response["LastPull"] = _lastPull;
            apiContext.Response["LastSuccessfulPull"] = _lastSuccessfulPull;
        }

        private void PullValues()
        {
            if (!Settings.IsEnabled)
            {
                Log.Verbose("Controller slave service is disabled.");
                return;
            }

            Log.Verbose($"Pulling values from master controller '{Settings.MasterAddress}'.");
            _lastPull = _dateTimeService.Now;

            try
            {
                PullOutsideTemperature();
                PullOutsideHumidity();
                PullDaylight();
                PullWeather();

                _lastSuccessfulPull = _dateTimeService.Now;
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unable to pull values from master controller.");
            }
        }

        private void PullWeather()
        {
            var response = PullValue(nameof(IWeatherService));
            var value = (string)response["Weather"];
            var weather = (Weather)Enum.Parse(typeof(Weather), value);

            if (Settings.UseWeather)
            {
                _weatherService.Update(weather);
            }
        }

        private void PullDaylight()
        {
            var response = PullValue(nameof(IDaylightService));
            var sunrise = (TimeSpan)response["Sunrise"];
            var sunset = (TimeSpan)response["Sunset"];

            if (Settings.UseSunriseSunset)
            {
                _daylightService.Update(sunrise, sunset);
            }
        }

        private void PullOutsideTemperature()
        {
            var response = PullValue(nameof(IOutdoorTemperatureService));
            var outdoorTemperature = (float)response["OutdoorTemperature"];

            if (Settings.UseTemperature)
            {
                _outdoorTemperatureService.Update(outdoorTemperature);
            }
        }

        private void PullOutsideHumidity()
        {
            var response = PullValue(nameof(IOutdoorHumidityService));

            var outdoorHumidity = (float)response["OutdoorHumidity"];
            if (Settings.UseHumidity)
            {
                _outdoorHumidityService.Update(outdoorHumidity);
            }
        }

        private JObject PullValue(string serviceName)
        {
            var uri = new Uri($"http://{Settings.MasterAddress}:80/api/Service/{serviceName}");
            using (var webClient = new HttpClient())
            {
                var body = webClient.GetStringAsync(uri).Result;
                if (body == null)
                {
                    throw new Exception($"Received no response from '{uri}'.");
                }

                return JObject.Parse(body);
            }
        }
    }
}
