using System;
using FluentAssertions;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Automations.Tests
{
    [TestClass]
    public class AutomationTests
    {
        [TestMethod]
        public void Automation_Toggle()
        {
            var testButton = new TestButton();
            var testOutput = new TestBinaryStateOutputActuator();

            CreateAutomation()
                .WithTrigger(testButton.GetPressedShortlyTrigger())
                .WithActionIfConditionsFulfilled(testOutput.GetToggleStateAction());

            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.Off);
            testButton.PressShort();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.On);
            testButton.PressShort();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.Off);
            testButton.PressShort();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.On);
        }

        [TestMethod]
        public void Automation_WithCondition()
        {
            var testController = new TestController();
            var automation = new Automation(AutomationIdFactory.EmptyId, testController.ApiController);

            var testButton = new TestButton();
            var testOutput = new TestBinaryStateOutputActuator();

            automation
                .WithTrigger(testButton.GetPressedShortlyTrigger())
                .WithCondition(ConditionRelation.And, new TimeRangeCondition(testController.Timer).WithStart(TimeSpan.FromHours(1)).WithEnd(TimeSpan.FromHours(2)))
                .WithActionIfConditionsFulfilled(testOutput.GetToggleStateAction());
            
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.Off);
            testController.SetTime(TimeSpan.FromHours(0));
            testButton.PressShort();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.Off);

            testController.SetTime(TimeSpan.FromHours(1.5));
            testButton.PressShort();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.On);
        }

        private Automation CreateAutomation()
        {
            var testController = new TestController();
            return new Automation(AutomationIdFactory.EmptyId, testController.ApiController);
        }
    }
}
