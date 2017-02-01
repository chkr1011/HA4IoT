using FluentAssertions;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
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
            var endpoint = new TestBinaryStateAdapter();
            var socket = new Socket(ComponentIdGenerator.EmptyId, endpoint);
            socket.ResetState();
            
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            socket.TryTurnOn();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetState().ShouldBeEquivalentTo(BinaryStateId.On);

            socket.TryTurnOn();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetState().ShouldBeEquivalentTo(BinaryStateId.On);

            socket.TryTurnOff();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            socket.TryTurnOff();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            socket.TryTurnOn();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(2);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }

        [TestMethod]
        public void Toggle_Lamp()
        {
            var adapter = new TestBinaryStateAdapter();
            var lamp = new Lamp(ComponentIdGenerator.EmptyId, adapter);
            lamp.ResetState();

            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            lamp.SetNextState();
            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetState().ShouldBeEquivalentTo(BinaryStateId.On);

            lamp.SetNextState();
            adapter.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            adapter.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            lamp.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
        }

        [TestMethod]
        public void StateAlias_Lamp()
        {
            var endpoint = new TestBinaryStateAdapter();
            var lamp = new Lamp(ComponentIdGenerator.EmptyId, endpoint);
            lamp.ResetState();

            lamp.SetStateIdAlias(BinaryStateId.On, LevelStateId.Level1);

            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            lamp.SetState(LevelStateId.Level1);
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }
    }
}
