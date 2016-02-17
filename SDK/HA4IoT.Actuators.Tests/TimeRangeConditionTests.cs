using System;
using FluentAssertions;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Actuators.Tests
{
    [TestClass]
    public class TimeRangeConditionTests
    {
        [TestMethod]
        public void NightRange_ShouldBeFulfilled()
        {
            var condition = new Condition().WithExpression(() => true);
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void DayRange_IN_RANGE_ShouldBeFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("10:00");

            var condition = new TimeRangeCondition(timer)
                .WithStart(TimeSpan.Parse("08:00"))
                .WithEnd(TimeSpan.Parse("18:00"));
                
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void DayRange_OUT_OF_RANGE_ShouldBeNotFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("21:00");

            var condition = new TimeRangeCondition(timer)
                .WithStart(TimeSpan.Parse("08:00"))
                .WithEnd(TimeSpan.Parse("18:00"));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void NightRange_IN_RANGE_ShouldBeFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = new TimeSpan(21, 00, 00, 00);

            var condition = new TimeRangeCondition(timer)
                .WithStart(TimeSpan.Parse("18:00"))
                .WithEnd(TimeSpan.Parse("08:00"));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void NightRange_OUT_OF_RANGE_ShouldBeNotFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("15:00");

            var condition = new TimeRangeCondition(timer)
                .WithStart(TimeSpan.Parse("18:00"))
                .WithEnd(TimeSpan.Parse("08:00"));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void AdjustedRange_OUT_OF_RANGE_ShouldBeNotFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("15:00");

            // 10-18 adjusted to 16-18
            var condition = new TimeRangeCondition(timer)
                .WithStart(TimeSpan.Parse("10:00"))
                .WithEnd(TimeSpan.Parse("18:00"))
                .WithStartAdjustment(TimeSpan.FromHours(6));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void AdjustedRange_IN_RANGE_ShouldBeFulfilled()
        {
            var timer = new TestHomeAutomationTimer();
            timer.CurrentTime = TimeSpan.Parse("17:00");

            // 10-18 adjusted to 16-18
            var condition = new TimeRangeCondition(timer)
                .WithStart(TimeSpan.Parse("10:00"))
                .WithEnd(TimeSpan.Parse("18:00"))
                .WithStartAdjustment(TimeSpan.FromHours(6));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }
    }
}
