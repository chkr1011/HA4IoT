using FluentAssertions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Automations.Tests
{
    [TestClass]
    public class ConditionalOnAutomationTests
    {
        [TestMethod]
        public void Empty_ConditionalOnAutomation()
        {
            var testController = new TestController();
            var automation = new ConditionalOnAutomation(AutomationIdFactory.EmptyId, testController.Timer, testController.ApiController);

            var testButton = new TestButton();
            var testOutput = new TestBinaryStateOutputActuator();

            automation.WithTrigger(testButton.GetPressedShortlyTrigger());
            automation.WithActuator(testOutput);
            
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.Off);
            testButton.PressShort();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.On);
        }
    }
}
