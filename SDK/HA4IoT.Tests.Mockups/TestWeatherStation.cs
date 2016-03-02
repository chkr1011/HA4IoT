using System;
using Windows.Data.Json;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Tests.Mockups
{
    public class TestWeatherStation : IWeatherStation
    {
        public DeviceId Id { get; set; }

        public JsonObject ExportStatusToJsonObject()
        {
            throw new NotSupportedException();
        }

        public Daylight Daylight { get; }
        public IWeatherSituationSensor SituationSensor { get; }
        public ITemperatureSensor TemperatureSensor { get; }
        public IHumiditySensor HumiditySensor { get; }
    }
}
