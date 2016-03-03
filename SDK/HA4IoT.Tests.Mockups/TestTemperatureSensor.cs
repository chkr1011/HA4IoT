using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Tests.Mockups
{
    public class TestTemperatureSensor : TestSensor, ITemperatureSensor
    {
        public TestTemperatureSensor(ActuatorId id, ILogger logger) : base(id, logger)
        {
        }
    }
}
