using FluentAssertions;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.Test;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Actuators.Tests
{
    [TestClass]
    public class BinaryStateOutputTests
    {
        [TestMethod]
        public void Written_ON_ShouldBeResultIn_HIGH_Port()
        {
            var port = new TestOutputPort();
            var output = new BinaryStateOutputActuator(new ActuatorId("test"), port, new TestHttpRequestController(), new TestLogger());

            output.TurnOn();

            port.GetInternalState().ShouldBeEquivalentTo(BinaryState.High);
        }

        [TestMethod]
        public void Written_OFF_ShouldBeResultIn_LOW_Port()
        {
            var port = new TestOutputPort();
            var output = new BinaryStateOutputActuator(new ActuatorId("test"), port, new TestHttpRequestController(), new TestLogger());

            output.TurnOff();

            port.GetInternalState().ShouldBeEquivalentTo(BinaryState.Low);
        }
    }
}
