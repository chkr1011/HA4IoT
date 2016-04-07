using System;
using FluentAssertions;
using HA4IoT.Actuators.StateMachines;
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
            var timer = new TestHomeAutomationTimer();
            var testButtonFactory = new TestButtonFactory(timer);
            var testStateMachineFactory = new TestStateMachineFactory();

            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = testStateMachineFactory.CreateTestStateMachineWithOnOffStates();

            CreateAutomation()
                .WithTrigger(testButton.GetPressedShortlyTrigger())
                .WithActionIfConditionsFulfilled(testOutput.GetSetNextStateAction());

            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);
            testButton.PressShortly();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);
            testButton.PressShortly();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);
            testButton.PressShortly();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);
        }

        [TestMethod]
        public void Automation_WithCondition()
        {
            var testController = new TestController();
            var automation = new Automation(AutomationIdFactory.EmptyId);

            var testButtonFactory = new TestButtonFactory(testController.Timer);
            var testStateMachineFactory = new TestStateMachineFactory();

            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = testStateMachineFactory.CreateTestStateMachineWithOnOffStates();

            automation
                .WithTrigger(testButton.GetPressedShortlyTrigger())
                .WithCondition(ConditionRelation.And, new TimeRangeCondition(testController.Timer).WithStart(TimeSpan.FromHours(1)).WithEnd(TimeSpan.FromHours(2)))
                .WithActionIfConditionsFulfilled(testOutput.GetSetNextStateAction());
            
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);
            testController.SetTime(TimeSpan.FromHours(0));
            testButton.PressShortly();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            testController.SetTime(TimeSpan.FromHours(1.5));
            testButton.PressShortly();
            testOutput.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);
        }

        private Automation CreateAutomation()
        {
            return new Automation(AutomationIdFactory.EmptyId);
        }
    }
}
