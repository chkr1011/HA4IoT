using System;
using System.Net.Http;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Environment;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Environment
{
    [ApiServiceClass(typeof(ControllerSlaveService))]
    public class ControllerSlaveService : ServiceBase
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IOutdoorService _outdoorService;
        private readonly IDaylightService _daylightService;
        private readonly ILogger _log;

        private DateTime? _lastPull;
        private DateTime? _lastSuccessfulPull;

        public ControllerSlaveService(
            ISettingsService settingsService,
            ISchedulerService scheduler,
            IDateTimeService dateTimeService,
            IOutdoorService outdoorService,
            IDaylightService daylightService,
            ILogService logService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _outdoorService = outdoorService ?? throw new ArgumentNullException(nameof(outdoorService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));

            _log = logService?.CreatePublisher(nameof(ControllerSlaveService)) ?? throw new ArgumentNullException(nameof(logService));

            settingsService.CreateSettingsMonitor<ControllerSlaveServiceSettings>(s => Settings = s.NewSettings);

            scheduler.Register("ControllerSlavePolling", TimeSpan.FromMinutes(5), () => PullValues());
        }

        public ControllerSlaveServiceSettings Settings { get; private set; }

        [ApiMethod]
        public void Status(IApiCall apiCall)
        {
            apiCall.Result["LastPull"] = _lastPull;
            apiCall.Result["LastSuccessfulPull"] = _lastSuccessfulPull;
        }

        private void PullValues()
        {
            if (!Settings.IsEnabled)
            {
                _log.Verbose("Controller slave service is disabled.");
                return;
            }

            _log.Verbose($"Pulling values from master controller '{Settings.MasterAddress}'.");
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
                _log.Warning(exception, "Unable to pull values from master controller.");
            }
        }

        private void PullWeather()
        {
            var response = PullValue(nameof(IOutdoorService));
            var value = (string)response["Weather"];
            var weather = (WeatherCondition)Enum.Parse(typeof(WeatherCondition), value);

            if (Settings.UseWeather)
            {
                _outdoorService.UpdateCondition(weather);
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
            var response = PullValue(nameof(IOutdoorService));
            var outdoorTemperature = (float)response["OutdoorTemperature"];

            if (Settings.UseTemperature)
            {
                _outdoorService.UpdateTemperature(outdoorTemperature);
            }
        }

        private void PullOutsideHumidity()
        {
            var response = PullValue(nameof(IOutdoorService));

            var outdoorHumidity = (float)response["OutdoorHumidity"];
            if (Settings.UseHumidity)
            {
                _outdoorService.UpdateHumidity(outdoorHumidity);
            }
        }

        private JObject PullValue(string serviceName)
        {
            var uri = new Uri($"http://{Settings.MasterAddress}:80/api/Service/{serviceName}/GetStatus");
            using (var webClient = new HttpClient())
            {
                var body = webClient.GetStringAsync(uri).Result;
                if (body == null)
                {
                    throw new Exception($"Received no response from '{uri}'.");
                }
                
                var response = JsonConvert.DeserializeObject<ApiResponse>(body);
                if (response == null)
                {
                    throw new Exception("Response has an invalid format.");    
                }

                if (response.ResultCode != ApiResultCode.Success)
                {
                    throw new Exception("API call failed.");
                }

                return response.Result;
            }
        }
    }
}
