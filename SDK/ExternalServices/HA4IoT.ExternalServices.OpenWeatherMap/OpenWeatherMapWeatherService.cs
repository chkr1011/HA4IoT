using Windows.Data.Json;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Networking;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapWeatherService : ServiceBase, IWeatherService
    {
        private WeatherSituation _weather;
        
        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedString("Weather", _weather.ToString());

            return status;
        }

        public WeatherSituation GetWeather()
        {
            return _weather;
        }

        internal void Update(WeatherSituation weather)
        {
            _weather = weather;
        }
    }
}
