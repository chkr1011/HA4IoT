using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Tests.Mockups;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace CK.HomeAutomation.Actuators.Tests
{
    [TestClass]
    public class BinaryStateOutputTests
    {
        [TestMethod]
        public void Written_ON_ShouldBeResultIn_HIGH_Port()
        {
            var port = new DummyOutputPort();
            var output = new BinaryStateOutput("test", port, new TestHttpRequestController(), new TestNotificationHandler());

            output.TurnOn();

            port.GetInternalState().ShouldBeEquivalentTo(BinaryState.High);
        }

        [TestMethod]
        public void Written_OFF_ShouldBeResultIn_LOW_Port()
        {
            var port = new DummyOutputPort();
            var output = new BinaryStateOutput("test", port, new TestHttpRequestController(), new TestNotificationHandler());

            output.TurnOff();

            port.GetInternalState().ShouldBeEquivalentTo(BinaryState.Low);
        }
    }
}
