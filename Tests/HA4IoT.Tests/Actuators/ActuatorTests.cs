using FluentAssertions;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
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

            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetState().HasState(PowerState.Off);

            socket.TryTurnOn();
            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetState().HasState(PowerState.On);

            socket.TryTurnOn();
            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetState().HasState(PowerState.On);

            socket.TryTurnOff();
            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetState().HasState(PowerState.Off);

            socket.TryTurnOff();
            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetState().HasState(PowerState.Off);

            socket.TryTurnOn();
            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(2);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetState().HasState(PowerState.On);
        }

        [TestMethod]
        public void Toggle_Lamp()
        {
            var adapter = new TestBinaryStateAdapter();
            var lamp = new Lamp(new ComponentId("Test"), adapter);
            lamp.ResetState();

            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetState().HasState(BinaryStateId.Off);

            lamp.TogglePowerStateAction.Execute();
            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetState().HasState(BinaryStateId.On);

            lamp.TogglePowerStateAction.Execute();
            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            lamp.GetState().HasState(BinaryStateId.Off);
        }
    }
}
