using Windows.Data.Json;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Networking;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapOutdoorHumidityService : ServiceBase, IOutdoorHumidityService
    {
        private float _outdoorHumidity;

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedNumber("OutdoorTemperature", _outdoorHumidity);

            return status;
        }

        public float GetOutdoorHumidity()
        {
            return _outdoorHumidity;
        }

        internal void Update(float outdoorHumidity)
        {
            _outdoorHumidity = outdoorHumidity;
        }
    }
}
