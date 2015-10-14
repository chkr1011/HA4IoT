using System;
using CK.HomeAutomation.Actuators.Automations;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Tests.Mockups;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace CK.HomeAutomation.Actuators.Tests
{
    [TestClass]
    public class AutomaticTurnOnAndOffAutomationTests
    {
        [TestMethod]
        public void Should_TurnOn_IfMotionDetected()
        {
            var timer = new TestHomeAutomationTimer();
            var automation = new AutomaticTurnOnAndOffAutomation(timer);
            var motionDetector = new TestMotionDetector();
            var output = new TestBinaryStateOutputActuator();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            motionDetector.WalkIntoMotionDetector();

            output.State.ShouldBeEquivalentTo(BinaryActuatorState.On);
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressedShort()
        {
            var timer = new TestHomeAutomationTimer();
            var automation = new AutomaticTurnOnAndOffAutomation(timer);
            var button = new TestButton();
            var output = new TestBinaryStateOutputActuator();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);

            automation.WithTrigger(button);
            automation.WithTarget(output);

            button.PressShort();

            output.State.ShouldBeEquivalentTo(BinaryActuatorState.On);
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndTimeRangeConditionIs_NotFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("18:00:00");

            var automation = new AutomaticTurnOnAndOffAutomation(timer);
            var motionDetector = new TestMotionDetector();
            var output = new TestBinaryStateOutputActuator();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            motionDetector.WalkIntoMotionDetector();

            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressed_EvenIfTimeRangeConditionIs_NotFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("18:00:00");

            var automation = new AutomaticTurnOnAndOffAutomation(timer);
            var button = new TestButton();
            var output = new TestBinaryStateOutputActuator();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithTrigger(button);
            automation.WithTarget(output);

            button.PressShort();

            output.State.ShouldBeEquivalentTo(BinaryActuatorState.On);
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndSkipConditionIs_Fulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("14:00:00");

            var automation = new AutomaticTurnOnAndOffAutomation(timer);
            var motionDetector = new TestMotionDetector();

            var output = new TestBinaryStateOutputActuator();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);
            automation.WithOnDuration(TimeSpan.FromSeconds(15));

            IBinaryStateOutputActuator[] otherActuators =
            {
                new TestBinaryStateOutputActuator().WithOffState(), new TestBinaryStateOutputActuator().WithOnState()
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            motionDetector.WalkIntoMotionDetector();

            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);
        }

        [TestMethod]
        public void Should_TurnOn_IfMotionDetected_AndSkipConditionIs_NotFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("14:00:00");

            var automation = new AutomaticTurnOnAndOffAutomation(timer);
            var motionDetector = new TestMotionDetector();

            var output = new TestBinaryStateOutputActuator();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            IBinaryStateOutputActuator[] otherActuators =
            {
                new TestBinaryStateOutputActuator().WithOffState(), new TestBinaryStateOutputActuator().WithOffState()
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            motionDetector.WalkIntoMotionDetector();

            output.State.ShouldBeEquivalentTo(BinaryActuatorState.On);
        }

        [TestMethod]
        public void Should_TurnOff_IfButtonPressed_WhileTargetIsAlreadyOn()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("14:00:00");

            var automation = new AutomaticTurnOnAndOffAutomation(timer);
            var button = new TestButton();

            var output = new TestBinaryStateOutputActuator();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);

            automation.WithTrigger(button);
            automation.WithTarget(output);

            IBinaryStateOutputActuator[] otherActuators =
            {
                new TestBinaryStateOutputActuator().WithOffState(), new TestBinaryStateOutputActuator().WithOffState()
            };

            automation.WithSkipIfAnyActuatorIsAlreadyOn(otherActuators);

            button.PressShort();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.On);

            button.PressShort();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.On);

            automation.WithTurnOffIfButtonPressedWhileAlreadyOn();
            button.PressShort();
            output.State.ShouldBeEquivalentTo(BinaryActuatorState.Off);
        }
    }
}
