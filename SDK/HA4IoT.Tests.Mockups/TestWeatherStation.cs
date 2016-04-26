using System;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.WeatherService;

namespace HA4IoT.Tests.Mockups
{
    public class TestWeatherStation : ServiceBase, IWeatherService
    {
        private readonly IHomeAutomationTimer _timer;

        private float _temperature;

        private float _humidity;

        private WeatherSituation _situation;

        public TestWeatherStation(DeviceId id, IHomeAutomationTimer timer)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            Id = id;
            _timer = timer;

            Sunrise = TimeSpan.Parse("06:00");
            Sunset = TimeSpan.Parse("18:00");
        }

        public DeviceId Id { get; }

        public TimeSpan Sunrise { get; set; }
        public TimeSpan Sunset { get; set; }
        public Daylight Daylight => new Daylight(_timer.CurrentTime, Sunrise, Sunset);

        public void SetTemperature(float value)
        {
            _temperature = value;
        }

        public void SetHumidity(float value)
        {
            _humidity = value;
        }

        public WeatherSituation GetSituation()
        {
            return _situation;
        }

        public float GetTemperature()
        {
            return _temperature;
        }

        public float GetHumidity()
        {
            return _humidity;
        }
    }
}
