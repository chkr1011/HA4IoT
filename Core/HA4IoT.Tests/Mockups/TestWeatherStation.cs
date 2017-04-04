using System;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Weather;

namespace HA4IoT.Tests.Mockups
{
    public class TestWeatherStation : ServiceBase, IWeatherService, IOutdoorTemperatureService, IOutdoorHumidityService
    {
        public Weather Weather { get; set; }

        DateTime? IWeatherService.Timestamp { get; }

        void IOutdoorHumidityService.Update(float outdoorHumidity)
        {
        }

        void IOutdoorTemperatureService.Update(float outdoorTemperature)
        {
        }

        public void Update(Weather weather)
        {
        }

        public float OutdoorTemperature { get; set; }

        DateTime? IOutdoorTemperatureService.Timestamp { get; }

        public float OutdoorHumidity { get; set; }

        DateTime? IOutdoorHumidityService.Timestamp { get; }
    }
}
