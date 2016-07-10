using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Weather;

namespace HA4IoT.Tests.Mockups
{
    public class TestWeatherStation : ServiceBase, IWeatherService, IOutdoorTemperatureService, IOutdoorHumidityService
    {
        private float _temperature;
        private float _humidity;
        private WeatherSituation _weather;

        public void SetTemperature(float value)
        {
            _temperature = value;
        }

        public void SetHumidity(float value)
        {
            _humidity = value;
        }

        public void SetWeather(WeatherSituation weather)
        {
            _weather = weather;
        }

        public WeatherSituation GetWeather()
        {
            return _weather;
        }

        public float GetOutdoorTemperature()
        {
            return _temperature;
        }

        public float GetOutdoorHumidity()
        {
            return _humidity;
        }
    }
}
