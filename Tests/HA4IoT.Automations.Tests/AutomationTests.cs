using System;
using FluentAssertions;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Services.System;
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
            var timer = new TestTimerService();
            var testButtonFactory = new TestButtonFactory(timer);
            var testStateMachineFactory = new TestStateMachineFactory();

            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = testStateMachineFactory.CreateTestStateMachineWithOnOffStates();

            CreateAutomation()
                .WithTrigger(testButton.GetPressedShortlyTrigger())
                .WithActionIfConditionsFulfilled(testOutput.GetSetNextStateAction());

            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }

        [TestMethod]
        public void Automation_WithCondition()
        {
            var testController = new TestController();
            var automation = new Automation(AutomationIdFactory.EmptyId);

            var testButtonFactory = new TestButtonFactory(testController.TimerService);
            var testStateMachineFactory = new TestStateMachineFactory();

            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = testStateMachineFactory.CreateTestStateMachineWithOnOffStates();

            automation
                .WithTrigger(testButton.GetPressedShortlyTrigger())
                .WithCondition(ConditionRelation.And, new TimeRangeCondition(testController.ServiceLocator.GetService<IDateTimeService>()).WithStart(TimeSpan.FromHours(1)).WithEnd(TimeSpan.FromHours(2)))
                .WithActionIfConditionsFulfilled(testOutput.GetSetNextStateAction());
            
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
            testController.SetTime(TimeSpan.FromHours(0));
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            testController.SetTime(TimeSpan.FromHours(1.5));
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }

        private Automation CreateAutomation()
        {
            return new Automation(AutomationIdFactory.EmptyId);
        }
    }
}
