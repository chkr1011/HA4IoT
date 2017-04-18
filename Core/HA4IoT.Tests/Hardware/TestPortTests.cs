using HA4IoT.Contracts.Hardware;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Hardware
{
    [TestClass]
    public class TestPortTests
    {
        [TestMethod]
        public void LOW_DummyInputPort_ShouldReturn_LOW()
        {
            var dummyPort = new TestPort();
            dummyPort.SetInternalState(BinaryState.Low);
            Assert.AreEqual(BinaryState.Low, dummyPort.Read());
        }

        [TestMethod]
        public void HIGH_DummyInputPort_ShouldReturn_HIGH()
        {
            var dummyPort = new TestPort();
            dummyPort.SetInternalState(BinaryState.High);
            Assert.AreEqual(BinaryState.High, dummyPort.Read());
        }

        [TestMethod]
        public void LOW_INVERTED_DummyInputPort_StateShouldReturn_HIGH()
        {
            var dummyPort = new TestPort { IsStateInverted = true };
            dummyPort.SetInternalState(BinaryState.Low);
            Assert.AreEqual(BinaryState.High, dummyPort.Read());
        }

        [TestMethod]
        public void HIGH_INVERTED_DummyInputPort_ShouldReturn_LOW()
        {
            var dummyPort = new TestPort { IsStateInverted = true };
            dummyPort.SetInternalState(BinaryState.High);
            Assert.AreEqual(BinaryState.Low, dummyPort.Read());
        }

        [TestMethod]
        public void DummyOutputPort_WithWrittenLOW_ShouldBeInternal_LOW()
        {
            var dummyPort = new TestPort();
            dummyPort.Write(BinaryState.Low);
            Assert.AreEqual(BinaryState.Low, dummyPort.GetInternalState());
        }

        [TestMethod]
        public void DummyOutputPort_WithWrittenHigh_ShouldBeInternal_HIGH()
        {
            var dummyPort = new TestPort();
            dummyPort.Write(BinaryState.High);
            Assert.AreEqual(BinaryState.High, dummyPort.GetInternalState());
        }

        [TestMethod]
        public void INVERTED_DummyOutputPort_WithWrittenLOW_ShouldBeInternal_HIGH()
        {
            var dummyPort = new TestPort { IsStateInverted = true };
            dummyPort.Write(BinaryState.Low);
            Assert.AreEqual(BinaryState.High, dummyPort.GetInternalState());
        }

        [TestMethod]
        public void INVERTED_DummyOutputPort_WithWrittenHIGH_ShouldBeInternal_LOW()
        {
            var dummyPort = new TestPort { IsStateInverted = true };
            dummyPort.Write(BinaryState.High);
            Assert.AreEqual(BinaryState.Low, dummyPort.GetInternalState());
        }
    }
}
