using Windows.Data.Json;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Networking;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapOutdoorTemperatureService : ServiceBase, IOutdoorTemperatureService
    {
        private float _outdoorTemperature;
        
        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedNumber("OutdoorTemperature", _outdoorTemperature);

            return status;
        }

        public float GetOutdoorTemperature()
        {
            return _outdoorTemperature;
        }

        internal void Update(float outdoorTemperature)
        {
            _outdoorTemperature = outdoorTemperature;
        }
    }
}
