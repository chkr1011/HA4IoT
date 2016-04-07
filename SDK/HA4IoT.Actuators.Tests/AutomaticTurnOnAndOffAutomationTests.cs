using System;
using FluentAssertions;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Actuators.Tests
{
    [TestClass]
    public class AutomaticTurnOnAndOffAutomationTests
    {
        [TestMethod]
        public void Should_TurnOn_IfMotionDetected()
        {
            var timer = new TestHomeAutomationTimer();
            var motionDetectorFactory = new TestMotionDetectorFactory(timer);
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, timer);
            var motionDetector = motionDetectorFactory.CreateTestMotionDetector();
            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            motionDetector.TriggerMotionDetection();

            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressedShort()
        {
            var timer = new TestHomeAutomationTimer();
            var buttonFactory = new TestButtonFactory(timer);
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, new TestHomeAutomationTimer());
            var button = buttonFactory.CreateTestButton();
            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            automation.WithTrigger(button.GetPressedShortlyTrigger());
            automation.WithTarget(output);

            button.PressShortly();

            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndTimeRangeConditionIs_NotFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.SetTime(TimeSpan.Parse("18:00:00"));

            var motionDetectorFactory = new TestMotionDetectorFactory(timer);
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, timer);
            var motionDetector = motionDetectorFactory.CreateTestMotionDetector();
            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            motionDetector.TriggerMotionDetection();

            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressed_EvenIfTimeRangeConditionIs_NotFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.SetTime(TimeSpan.Parse("18:00:00"));

            var buttonFactory = new TestButtonFactory(timer);
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, timer);
            var button = buttonFactory.CreateTestButton();
            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithTrigger(button.GetPressedShortlyTrigger());
            automation.WithTarget(output);

            button.PressShortly();

            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndSkipConditionIs_Fulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.SetTime(TimeSpan.Parse("14:00:00"));

            var motionDetectorFactory = new TestMotionDetectorFactory(timer);
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, timer);
            var motionDetector = motionDetectorFactory.CreateTestMotionDetector();

            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);
            automation.WithOnDuration(TimeSpan.FromSeconds(15));

            IStateMachine[] otherActuators =
            {
                stateMachineFactory.CreateTestStateMachineWithOnOffStates(),
                stateMachineFactory.CreateTestStateMachineWithOnOffStates(DefaultStateId.On)
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            motionDetector.TriggerMotionDetection();

            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);
        }

        [TestMethod]
        public void Should_TurnOn_IfMotionDetected_AndSkipConditionIs_NotFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.SetTime(TimeSpan.Parse("14:00:00"));

            var motionDetectorFactory = new TestMotionDetectorFactory(timer);
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, timer);
            var motionDetector = motionDetectorFactory.CreateTestMotionDetector();

            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            IStateMachine[] otherActuators =
            {
                stateMachineFactory.CreateTestStateMachineWithOnOffStates(),
                stateMachineFactory.CreateTestStateMachineWithOnOffStates()
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            motionDetector.TriggerMotionDetection();

            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);
        }

        [TestMethod]
        public void Should_TurnOff_IfButtonPressed_WhileTargetIsAlreadyOn()
        {
            var timer = new TestHomeAutomationTimer();
            timer.SetTime(TimeSpan.Parse("14:00:00"));

            var buttonFactory = new TestButtonFactory(timer);
            var stateMachineFactory = new TestStateMachineFactory();
            
            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, timer);
            var button = buttonFactory.CreateTestButton();

            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);

            automation.WithTrigger(button.GetPressedShortlyTrigger());
            automation.WithTarget(output);

            IStateMachine[] otherActuators =
            {
                stateMachineFactory.CreateTestStateMachineWithOnOffStates(),
                stateMachineFactory.CreateTestStateMachineWithOnOffStates()
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            button.PressShortly();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);

            button.PressShortly();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.On);

            automation.WithTurnOffIfButtonPressedWhileAlreadyOn();
            button.PressShortly();
            output.GetActiveState().ShouldBeEquivalentTo(DefaultStateId.Off);
        }
    }
}
