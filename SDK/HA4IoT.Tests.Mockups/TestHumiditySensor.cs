using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Tests.Mockups
{
    public class TestHumiditySensor : TestSensor, IHumiditySensor
    {
        public TestHumiditySensor(ActuatorId id) 
            : base(id)
        {
        }
    }
}
