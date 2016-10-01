using System;
using FluentAssertions;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.System;
using HA4IoT.Settings;
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
            var schedulerService = new SchedulerService(new TestTimerService(), new DateTimeService());
            var motionDetectorFactory = new TestMotionDetectorFactory(schedulerService, new SettingsService());
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, new TestDateTimeService(), schedulerService, new SettingsService(), new TestDaylightService());
            var motionDetector = motionDetectorFactory.CreateTestMotionDetector();
            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            motionDetector.TriggerMotionDetection();

            output.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressedShort()
        {
            var timer = new TestTimerService();
            var buttonFactory = new TestButtonFactory(timer, new SettingsService());
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, new TestDateTimeService(), new SchedulerService(new TestTimerService(), new DateTimeService()), new SettingsService(), new TestDaylightService());
            var button = buttonFactory.CreateTestButton();
            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            automation.WithTrigger(button.GetPressedShortlyTrigger());
            automation.WithTarget(output);

            button.PressShortly();

            output.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndTimeRangeConditionIs_NotFulfilled()
        {
            var timer = new TestTimerService();
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("18:00:00"));

            var motionDetectorFactory = new TestMotionDetectorFactory(new SchedulerService(timer, dateTimeService), new SettingsService());
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, dateTimeService, new SchedulerService(timer, dateTimeService), new SettingsService(), new TestDaylightService());
            var motionDetector = motionDetectorFactory.CreateTestMotionDetector();
            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            motionDetector.TriggerMotionDetection();

            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressed_EvenIfTimeRangeConditionIs_NotFulfilled()
        {
            var timer = new TestTimerService();
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("18:00:00"));

            var buttonFactory = new TestButtonFactory(timer, new SettingsService());
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, dateTimeService, new SchedulerService(timer, dateTimeService), new SettingsService(), new TestDaylightService());
            var button = buttonFactory.CreateTestButton();
            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithTrigger(button.GetPressedShortlyTrigger());
            automation.WithTarget(output);

            button.PressShortly();

            output.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndSkipConditionIs_Fulfilled()
        {
            var timer = new TestTimerService();
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("14:00:00"));

            var motionDetectorFactory = new TestMotionDetectorFactory(new SchedulerService(timer, dateTimeService), new SettingsService());
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, dateTimeService, new SchedulerService(timer, dateTimeService), new SettingsService(), new TestDaylightService());
            var motionDetector = motionDetectorFactory.CreateTestMotionDetector();

            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            IStateMachine[] otherActuators =
            {
                stateMachineFactory.CreateTestStateMachineWithOnOffStates(),
                stateMachineFactory.CreateTestStateMachineWithOnOffStates(BinaryStateId.On)
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            motionDetector.TriggerMotionDetection();

            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
        }

        [TestMethod]
        public void Should_TurnOn_IfMotionDetected_AndSkipConditionIs_NotFulfilled()
        {
            var timer = new TestTimerService();
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("14:00:00"));

            var motionDetectorFactory = new TestMotionDetectorFactory(new SchedulerService(timer, dateTimeService), new SettingsService());
            var stateMachineFactory = new TestStateMachineFactory();

            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, dateTimeService, new SchedulerService(timer, dateTimeService), new SettingsService(), new TestDaylightService());
            var motionDetector = motionDetectorFactory.CreateTestMotionDetector();

            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            IStateMachine[] otherActuators =
            {
                stateMachineFactory.CreateTestStateMachineWithOnOffStates(),
                stateMachineFactory.CreateTestStateMachineWithOnOffStates()
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            motionDetector.TriggerMotionDetection();

            output.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }

        [TestMethod]
        public void Should_TurnOff_IfButtonPressed_WhileTargetIsAlreadyOn()
        {
            var timer = new TestTimerService();
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("14:00:00"));

            var buttonFactory = new TestButtonFactory(timer, new SettingsService());
            var stateMachineFactory = new TestStateMachineFactory();
            
            var automation = new TurnOnAndOffAutomation(AutomationIdFactory.EmptyId, dateTimeService, new SchedulerService(timer, dateTimeService), new SettingsService(), new TestDaylightService());
            var button = buttonFactory.CreateTestButton();

            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);

            automation.WithTrigger(button.GetPressedShortlyTrigger());
            automation.WithTarget(output);

            IStateMachine[] otherActuators =
            {
                stateMachineFactory.CreateTestStateMachineWithOnOffStates(),
                stateMachineFactory.CreateTestStateMachineWithOnOffStates()
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            button.PressShortly();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.On);

            button.PressShortly();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.On);

            automation.WithTurnOffIfButtonPressedWhileAlreadyOn();
            button.PressShortly();
            output.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
        }
    }
}
