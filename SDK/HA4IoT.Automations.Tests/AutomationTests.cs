using System;
using FluentAssertions;
using HA4IoT.Actuators;
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
            var testOutput = new TestStateMachine();

            CreateAutomation()
                .WithTrigger(testButton.GetPressedShortlyTrigger())
                .WithActionIfConditionsFulfilled(testOutput.GetSetNextStateAction());

            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);
            testButton.PressShort();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.On);
            testButton.PressShort();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);
            testButton.PressShort();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.On);
        }

        [TestMethod]
        public void Automation_WithCondition()
        {
            var testController = new TestController();
            var automation = new Automation(AutomationIdFactory.EmptyId);

            var testButton = new TestButton();
            var testOutput = new TestStateMachine();

            automation
                .WithTrigger(testButton.GetPressedShortlyTrigger())
                .WithCondition(ConditionRelation.And, new TimeRangeCondition(testController.Timer).WithStart(TimeSpan.FromHours(1)).WithEnd(TimeSpan.FromHours(2)))
                .WithActionIfConditionsFulfilled(testOutput.GetSetNextStateAction());
            
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);
            testController.SetTime(TimeSpan.FromHours(0));
            testButton.PressShort();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.Off);

            testController.SetTime(TimeSpan.FromHours(1.5));
            testButton.PressShort();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateIDs.On);
        }

        private Automation CreateAutomation()
        {
            var testController = new TestController();
            return new Automation(AutomationIdFactory.EmptyId);
        }
    }
}
