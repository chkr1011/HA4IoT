using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Components;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Actuators
{
    [TestClass]
    public class ActuatorTests
    {
        [TestMethod]
        public void TurnOnAndTurnOff_Socket()
        {
            var adapter = new TestBinaryStateAdapter();
            var socket = new Socket(new ComponentId("Test"), adapter);
            socket.ResetState();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, socket.GetState().Has(PowerState.Off));

            socket.TryTurnOn();
            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, socket.GetState().Has(PowerState.On));

            socket.TryTurnOn();
            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, socket.GetState().Has(PowerState.On));

            socket.TryTurnOff();
            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(2, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, socket.GetState().Has(PowerState.Off));

            socket.TryTurnOff();
            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(2, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, socket.GetState().Has(PowerState.Off));

            socket.TryTurnOn();
            Assert.AreEqual(2, adapter.TurnOnCalledCount);
            Assert.AreEqual(2, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, socket.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Toggle_Lamp()
        {
            var adapter = new TestBinaryStateAdapter();
            var lamp = new Lamp(new ComponentId("Test"), adapter);
            lamp.ResetState();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));

            lamp.TogglePowerStateAction.Execute();
            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.On));

            lamp.TogglePowerStateAction.Execute();
            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(2, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));
        }
    }
}
