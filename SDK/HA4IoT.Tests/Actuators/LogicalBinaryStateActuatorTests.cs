using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Actuators
{
    [TestClass]
    public class LogicalBinaryStateActuatorTests
    {
        [TestMethod]
        public void CombinedComponent_TurnOnAndOff()
        {
            var timer = new TestTimerService();
            
            var testActuator1 = new Lamp(new ComponentId("Lamp1"), new TestBinaryStateAdapter());
            var testActuator2 = new Lamp(new ComponentId("Lamp2"), new TestBinaryStateAdapter());
            var testActuator3 = new Lamp(new ComponentId("Lamp3"), new TestBinaryStateAdapter());

            var logicalActautor = new LogicalBinaryStateActuator(ComponentIdGenerator.EmptyId, timer);
            logicalActautor.WithActuator(testActuator1);
            logicalActautor.WithActuator(testActuator2);
            logicalActautor.WithActuator(testActuator3);

            logicalActautor.InvokeCommand(new TurnOffCommand());
            Assert.IsTrue(logicalActautor.GetState().Has(PowerState.Off));

            logicalActautor.InvokeCommand(new TurnOnCommand());
            Assert.IsTrue(logicalActautor.GetState().Has(PowerState.On));

            logicalActautor.InvokeCommand(new TurnOffCommand());
            Assert.IsTrue(logicalActautor.GetState().Has(PowerState.Off));
        }
    }
}
