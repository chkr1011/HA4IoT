using FluentAssertions;
using HA4IoT.Contracts.Actuators;
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
            var socket = new Socket(ActuatorIdFactory.EmptyId, endpoint);
            
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);

            socket.TryTurnOn();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.On);

            socket.TryTurnOn();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.On);

            socket.TryTurnOff();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);

            socket.TryTurnOff();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);

            socket.TryTurnOn();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(2);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            socket.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.On);
        }

        [TestMethod]
        public void Toggle_Lamp()
        {
            var endpoint = new TestBinaryStateEndpoint();
            var lamp = new Lamp(ActuatorIdFactory.EmptyId, endpoint);

            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);

            lamp.SetNextState();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.On);

            lamp.SetNextState();
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(2);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);
        }

        [TestMethod]
        public void StateAlias_Lamp()
        {
            var endpoint = new TestBinaryStateEndpoint();
            var lamp = new Lamp(ActuatorIdFactory.EmptyId, endpoint);
            lamp.SetStateIdAlias(DefaultStateIDs.On, DefaultStateIDs.Level1);

            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(0);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);

            lamp.SetActiveState(DefaultStateIDs.Level1);
            endpoint.TurnOnCalledCount.ShouldBeEquivalentTo(1);
            endpoint.TurnOffCalledCount.ShouldBeEquivalentTo(1);
            lamp.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.On);
        }
    }
}
