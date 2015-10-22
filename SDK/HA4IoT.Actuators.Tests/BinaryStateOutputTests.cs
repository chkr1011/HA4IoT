using FluentAssertions;
using HA4IoT.Hardware;
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
            var output = new BinaryStateOutput("test", port, new TestHttpRequestController(), new TestNotificationHandler());

            output.TurnOn();

            port.GetInternalState().ShouldBeEquivalentTo(BinaryState.High);
        }

        [TestMethod]
        public void Written_OFF_ShouldBeResultIn_LOW_Port()
        {
            var port = new TestOutputPort();
            var output = new BinaryStateOutput("test", port, new TestHttpRequestController(), new TestNotificationHandler());

            output.TurnOff();

            port.GetInternalState().ShouldBeEquivalentTo(BinaryState.Low);
        }
    }
}
