using FluentAssertions;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Actuators.Tests
{
    [TestClass]
    public class ActuatorTests
    {
        [TestMethod]
        public void TurnOnAndTurnOff_Socket()
        {
            var endpoint = new TestBinaryStateEndpoint();
            var socket = new Socket(ComponentIdFactory.EmptyId, endpoint);
            
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            socket.TryTurnOn();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);

            socket.TryTurnOn();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);

            socket.TryTurnOff();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            socket.TryTurnOff();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            socket.TryTurnOn();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(2);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);
        }

        [TestMethod]
        public void Toggle_Lamp()
        {
            var endpoint = new TestBinaryStateEndpoint();
            var lamp = new Lamp(ComponentIdFactory.EmptyId, endpoint);

            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            lamp.SetNextState();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);

            lamp.SetNextState();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);
        }

        [TestMethod]
        public void StateAlias_Lamp()
        {
            var endpoint = new TestBinaryStateEndpoint();
            var lamp = new Lamp(ComponentIdFactory.EmptyId, endpoint);
            lamp.SetStateIdAlias(DefaultStateId.On, DefaultStateId.Level1);

            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            lamp.SetActiveState(DefaultStateId.Level1);
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);
        }
    }
}
