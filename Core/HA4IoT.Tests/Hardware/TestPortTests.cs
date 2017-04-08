using FluentAssertions;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Hardware
{
    [TestClass]
    public class TestPortTests
    {
        [TestMethod]
        public void LOW_DummyInputPort_ShouldReturn_LOW()
        {
            var dummyPort = new TestInputPort();
            dummyPort.SetInternalState(BinaryState.Low);
            dummyPort.Read().ShouldBeEquivalentTo(BinaryState.Low);
        }

        [TestMethod]
        public void HIGH_DummyInputPort_ShouldReturn_HIGH()
        {
            var dummyPort = new TestInputPort();
            dummyPort.SetInternalState(BinaryState.High);
            dummyPort.Read().ShouldBeEquivalentTo(BinaryState.High);
        }

        [TestMethod]
        public void LOW_INVERTED_DummyInputPort_StateShouldReturn_HIGH()
        {
            var dummyPort = new TestInputPort();
            dummyPort = (TestInputPort)dummyPort.WithInvertedState();
            dummyPort.SetInternalState(BinaryState.Low);
            dummyPort.Read().ShouldBeEquivalentTo(BinaryState.High);
        }

        [TestMethod]
        public void HIGH_INVERTED_DummyInputPort_ShouldReturn_LOW()
        {
            var dummyPort = new TestInputPort();
            dummyPort = (TestInputPort)dummyPort.WithInvertedState();
            dummyPort.SetInternalState(BinaryState.High);
            dummyPort.Read().ShouldBeEquivalentTo(BinaryState.Low);
        }
        
        [TestMethod]
        public void DummyOutputPort_WithWrittenLOW_ShouldBeInternal_LOW()
        {
            var dummyPort = new TestOutputPort();
            dummyPort.Write(BinaryState.Low);
            dummyPort.GetInternalState().ShouldBeEquivalentTo(BinaryState.Low);
        }

        [TestMethod]
        public void DummyOutputPort_WithWrittenHigh_ShouldBeInternal_HIGH()
        {
            var dummyPort = new TestOutputPort();
            dummyPort.Write(BinaryState.High);
            dummyPort.GetInternalState().ShouldBeEquivalentTo(BinaryState.High);
        }

        [TestMethod]
        public void INVERTED_DummyOutputPort_WithWrittenLOW_ShouldBeInternal_HIGH()
        {
            var dummyPort = new TestOutputPort();
            dummyPort = (TestOutputPort)dummyPort.WithInvertedState();
            dummyPort.Write(BinaryState.Low);
            dummyPort.GetInternalState().ShouldBeEquivalentTo(BinaryState.High);
        }

        [TestMethod]
        public void INVERTED_DummyOutputPort_WithWrittenHIGH_ShouldBeInternal_LOW()
        {
            var dummyPort = new TestOutputPort();
            dummyPort = (TestOutputPort)dummyPort.WithInvertedState();
            dummyPort.Write(BinaryState.High);
            dummyPort.GetInternalState().ShouldBeEquivalentTo(BinaryState.Low);
        }
    }
}
