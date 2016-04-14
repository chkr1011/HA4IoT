using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Sensors.HumiditySensors;

namespace HA4IoT.Tests.Mockups
{
    public class TestHumiditySensor : HumiditySensor
    {
        public TestHumiditySensor(ComponentId id, TestNumericValueSensorEndpoint endpoint) 
            : base(id, endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint;
        }

        public TestNumericValueSensorEndpoint Endpoint{ get; }
    }
}
