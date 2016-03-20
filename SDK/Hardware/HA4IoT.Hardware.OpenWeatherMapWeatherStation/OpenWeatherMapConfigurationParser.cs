using System;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class OpenWeatherMapConfigurationParser
    {
        private static readonly string Filename = StoragePath.WithFilename("OpenWeatherMapConfiguration.json");

        public Uri GetUri()
        {
            double latitude = 0;
            double longitude = 0;
            string appId = null;

            JsonObject configuration;
            if (TryGetConfiguration(out configuration))
            {
                latitude = configuration.GetNamedNumber("lat", 0);
                longitude = configuration.GetNamedNumber("lon", 0);
                appId = configuration.GetNamedString("appID", string.Empty);
            }
            
            if (latitude == 0 || longitude == 0)
            {
                Log.Warning("Open Weather Map coordinates invalid.");
            }

            return new Uri($"http://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&APPID={appId}&units=metric");
        }

        public static bool ConfigurationFileExists()
        {
            return File.Exists(Filename);
        }

        private bool TryGetConfiguration(out JsonObject configuration)
        {
            configuration = null;
            
            if (!ConfigurationFileExists())
            {
                Log.Warning($"Open Weather Map configuration ({Filename}) not found.");
                return false;
            }

            try
            {
                configuration = JsonObject.Parse(File.ReadAllText(Filename));
                return true;
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unable to parse Open Weather Map configuration.");
            }

            return false;
        }
    }
}
