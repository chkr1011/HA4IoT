using System;
using FluentAssertions;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Conditions;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Services;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Actuators
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
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("10:00"));

            var condition = new TimeRangeCondition(dateTimeService)
                .WithStart(TimeSpan.Parse("08:00"))
                .WithEnd(TimeSpan.Parse("18:00"));
                
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void DayRange_OUT_OF_RANGE_ShouldBeNotFulfilled()
        {
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("21:00"));

            var condition = new TimeRangeCondition(dateTimeService)
                .WithStart(TimeSpan.Parse("08:00"))
                .WithEnd(TimeSpan.Parse("18:00"));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void NightRange_IN_RANGE_ShouldBeFulfilled()
        {
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("21:00"));

            var condition = new TimeRangeCondition(dateTimeService)
                .WithStart(TimeSpan.Parse("18:00"))
                .WithEnd(TimeSpan.Parse("08:00"));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void NightRange_OUT_OF_RANGE_ShouldBeNotFulfilled()
        {
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("15:00"));

            var condition = new TimeRangeCondition(dateTimeService)
                .WithStart(TimeSpan.Parse("18:00"))
                .WithEnd(TimeSpan.Parse("08:00"));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void AdjustedRange_OUT_OF_RANGE_ShouldBeNotFulfilled()
        {
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("15:00"));

            // 10-18 adjusted to 16-18
            var condition = new TimeRangeCondition(dateTimeService)
                .WithStart(TimeSpan.Parse("10:00"))
                .WithEnd(TimeSpan.Parse("18:00"))
                .WithStartAdjustment(TimeSpan.FromHours(6));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void AdjustedRange_IN_RANGE_ShouldBeFulfilled()
        {
            var dateTimeService = new TestDateTimeService();
            dateTimeService.SetTime(TimeSpan.Parse("17:00"));

            // 10-18 adjusted to 16-18
            var condition = new TimeRangeCondition(dateTimeService)
                .WithStart(TimeSpan.Parse("10:00"))
                .WithEnd(TimeSpan.Parse("18:00"))
                .WithStartAdjustment(TimeSpan.FromHours(6));

            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }
    }
}
