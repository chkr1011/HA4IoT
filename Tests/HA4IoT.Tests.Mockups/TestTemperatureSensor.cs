using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Sensors.TemperatureSensors;

namespace HA4IoT.Tests.Mockups
{
    public class TestTemperatureSensor : TemperatureSensor
    {
        public TestTemperatureSensor(ComponentId id, ISettingsService settingsService, TestNumericValueSensorEndpoint endpoint) 
            : base(id, settingsService, endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint;
        }

        public TestNumericValueSensorEndpoint Endpoint { get; }
    }
}
