using System;
using HA4IoT.Contracts.Environment;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Tests.Mockups
{
    public class TestWeatherStation : ServiceBase, IOutdoorService
    {
        public WeatherCondition Weather { get; set; }
        public DateTime? WeatherTimestamp { get; set; }
        public void UpdateWeather(WeatherCondition value)
        {
            Weather = value;
            WeatherTimestamp = DateTime.Now;
        }

        public float Humidity { get; set; }
        public DateTime? HumidityTimestamp { get; set; }
        public float Temperature { get; set; }
        public DateTime? TemperatureTimestamp { get; set;  }
        public void UpdateHumidity(float humidity)
        {
            Humidity = humidity;
            HumidityTimestamp = DateTime.Now;
        }

        public void UpdateTemperature(float temperature)
        {
            Temperature = temperature;
            TemperatureTimestamp = DateTime.Now;
        }
    }
}
