using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class OWMWeatherStationInitializer
    {
        private readonly IHomeAutomationTimer _timer;
        private readonly IHttpRequestController _httpApiController;
        private readonly ILogger _logger;

        public OWMWeatherStationInitializer(IHomeAutomationTimer timer, IHttpRequestController httpApiController, ILogger logger)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _timer = timer;
            _httpApiController = httpApiController;
            _logger = logger;
        }

        public OWMWeatherStation CreateWeatherStation()
        {
            return CreateWeatherStation(OWMWeatherStation.DefaultDeviceId);
        }

        public OWMWeatherStation CreateWeatherStation(DeviceId deviceId)
        {
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));

            JsonObject configuration = GetConfiguration();

            double lat = 0;
            double lon = 0;
            string appId = null;
            if (configuration != null)
            {
                lat = configuration.GetNamedNumber("lat", 0);
                lon = configuration.GetNamedNumber("lon", 0);
                appId = configuration.GetNamedString("appID", string.Empty);
            }
            
            if (lat == 0 || lon == 0)
            {
                _logger.Warning("OWM weather station coordinates invalid. Falling back to Berlin.");
            }
            
            var weatherStation = new OWMWeatherStation(deviceId, lat, lon, appId, _timer, _httpApiController, _logger);
            _logger.Info("WeatherStation initialized successfully");

            return weatherStation;
        }

        private JsonObject GetConfiguration()
        {
            string filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "WeatherStationConfiguration.json");
            if (!File.Exists(filename))
            {
                return null;
            }

            try
            {
                return JsonObject.Parse(File.ReadAllText(filename));
            }
            catch (Exception exception)
            {
                _logger.Warning(exception,
                    "Unable to initialize OWM weather station from configuration file. Falling back to Berlin.");

                return null;
            }
        }
    }
}
