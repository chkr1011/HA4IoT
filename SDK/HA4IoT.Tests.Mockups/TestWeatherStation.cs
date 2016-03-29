using System;
using Windows.Data.Json;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.WeatherService;

namespace HA4IoT.Tests.Mockups
{
    public class TestWeatherStation : IWeatherService
    {
        private readonly IHomeAutomationTimer _timer;

        public TestWeatherStation(DeviceId id, IHomeAutomationTimer timer)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            Id = id;
            _timer = timer;

            TemperatureSensor = new TestTemperatureSensor(new ActuatorId("Test.Temperature"));
            HumiditySensor = new TestHumiditySensor(new ActuatorId("Test.Humidity"));

            Sunrise = TimeSpan.Parse("06:00");
            Sunset = TimeSpan.Parse("18:00");
        }

        public DeviceId Id { get; }

        public JsonObject ExportStatusToJsonObject()
        {
            throw new NotSupportedException();
        }

        public TimeSpan Sunrise { get; set; }
        public TimeSpan Sunset { get; set; }

        public Daylight Daylight => new Daylight(_timer.CurrentTime, Sunrise, Sunset);
        public IWeatherSituationSensor SituationSensor { get; }
        public ITemperatureSensor TemperatureSensor { get; }
        public IHumiditySensor HumiditySensor { get; }

        public void SetTemperature(float value)
        {
            ((TestTemperatureSensor)TemperatureSensor).SetValue(value);
        }

        public void SetHumidity(float value)
        {
            ((TestHumiditySensor)HumiditySensor).SetValue(value);
        }
    }
}
