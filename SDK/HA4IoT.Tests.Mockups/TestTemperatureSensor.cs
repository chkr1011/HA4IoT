using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Tests.Mockups
{
    public class TestTemperatureSensor : TestSensor, ITemperatureSensor
    {
        public TestTemperatureSensor(ActuatorId id) 
            : base(id)
        {
        }
    }
}
