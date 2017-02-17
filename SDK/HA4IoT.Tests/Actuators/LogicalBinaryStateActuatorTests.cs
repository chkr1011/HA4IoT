using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Adapters;
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
            
            var testActuator1 = new Lamp("Lamp1", new TestBinaryOutputAdapter());
            var testActuator2 = new Lamp("Lamp2", new TestBinaryOutputAdapter());
            var testActuator3 = new Lamp("Lamp3", new TestBinaryOutputAdapter());

            var logicalActautor = new LogicalBinaryStateActuator("Test", timer);
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
