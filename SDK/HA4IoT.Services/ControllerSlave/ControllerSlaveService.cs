using System;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Weather;
using System.Net.Http;
using Windows.Data.Json;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;

namespace HA4IoT.Services.ControllerSlave
{
    public class ControllerSlaveService : ServiceBase, IOutdoorTemperatureProvider, IOutdoorHumidityProvider, IDaylightProvider, IWeatherProvider
    {
        private readonly ControllerSlaveServiceOptions _masterControllerAddress;
        private readonly IDateTimeService _dateTimeService;

        public event EventHandler<OutdoorTemperatureFetchedEventArgs> OutdoorTemperatureFetched;
        public event EventHandler<OutdoorHumidityFetchedEventArgs> OutdoorHumidityFetched;
        public event EventHandler<DaylightFetchedEventArgs> DaylightFetched;
        public event EventHandler<WeatherFetchedEventArgs> WeatherFetched;

        private DateTime? _lastPull;
        private DateTime? _lastSuccessfulPull;

        public ControllerSlaveService(ControllerSlaveServiceOptions options, ISchedulerService scheduler, IDateTimeService dateTimeService)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _masterControllerAddress = options;
            _dateTimeService = dateTimeService;

            scheduler.RegisterSchedule("ControllerSlavePolling", TimeSpan.FromMinutes(5), PullValues);
        }

        public override JsonObject GetStatus()
        {
            var status = base.GetStatus();
            status.SetNamedDateTime("LastPull", _lastPull);
            status.SetNamedDateTime("LastSuccessfulPull", _lastSuccessfulPull);

            return status;
        }

        private void PullValues()
        {
            Log.Verbose($"Pulling values from master controller '{_masterControllerAddress}'.");
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
            var response = PullValue("IWeatherService");
            var value = response.GetNamedString("Weather");
            var weather = (Weather)Enum.Parse(typeof(Weather), value);
            WeatherFetched?.Invoke(this, new WeatherFetchedEventArgs(weather));
        }

        private void PullDaylight()
        {
            var response = PullValue("IDaylightService");
            var sunrise = response.GetNamedTimeSpan("Sunrise").Value;
            var sunset = response.GetNamedTimeSpan("Sunset").Value;
            DaylightFetched?.Invoke(this, new DaylightFetchedEventArgs(sunrise, sunset));
        }

        private void PullOutsideTemperature()
        {
            var response = PullValue("IOutdoorTemperatureService");
            var outsideTemperature = (float)response.GetNamedNumber("OutdoorTemperature");
            OutdoorTemperatureFetched?.Invoke(this, new OutdoorTemperatureFetchedEventArgs(outsideTemperature));
        }

        private void PullOutsideHumidity()
        {
            var response = PullValue("IOutdoorHumidityService");
            var outsideTemperature = (float)response.GetNamedNumber("OutdoorHumidity");
            OutdoorHumidityFetched?.Invoke(this, new OutdoorHumidityFetchedEventArgs(outsideTemperature));
        }

        private JsonObject PullValue(string serviceName)
        {
            var uri = new Uri($"http://{_masterControllerAddress.MasterControllerAddress}:80/api/service/{serviceName}");
            using (var webClient = new HttpClient())
            {
                string body = webClient.GetStringAsync(uri).Result;

                if (body == null)
                {
                    throw new Exception($"Received no response from '{uri}'.");
                }

                return JsonObject.Parse(body);
            }
        }
    }
}
