using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Tests.Mockups
{
    public class TestHumiditySensor : TestSensor, IHumiditySensor
    {
        public TestHumiditySensor(ActuatorId id, ILogger logger) : base(id, logger)
        {
        }
    }
}
