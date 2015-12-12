using System;
using HA4IoT.Contracts;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Conditions.Specialized
{
    public class TimeRangeCondition : Condition
    {
        private readonly IHomeAutomationTimer _timer;

        private Func<TimeSpan> _startValueProvider;
        private Func<TimeSpan> _endValueProvider;

        public TimeRangeCondition(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
            WithExpression(() => Check());
        }

        private TimeSpan? StartAdjustment { get; set; }

        private TimeSpan? EndAdjustment { get; set; }

        public TimeRangeCondition WithStart(Func<TimeSpan> start)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));

            _startValueProvider = start;
            return this;
        }

        public TimeRangeCondition WithEnd(Func<TimeSpan> end)
        {
            if (end == null) throw new ArgumentNullException(nameof(end));

            _endValueProvider = end;
            return this;
        }

        public TimeRangeCondition WithStart(TimeSpan start)
        {
            _startValueProvider = () => start;
            return this;
        }

        public TimeRangeCondition WithEnd(TimeSpan end)
        {
            _endValueProvider = () => end;
            return this;
        }

        public TimeRangeCondition WithStartAdjustment(TimeSpan value)
        {
            StartAdjustment = value;
            return this;
        }

        public TimeRangeCondition WithEndAdjustment(TimeSpan value)
        {
            EndAdjustment = value;
            return this;
        }

        private ConditionState Check()
        {
            if (_startValueProvider == null || _endValueProvider == null)
            {
                return ConditionState.NotFulfilled;
            }

            TimeSpan startValue = _startValueProvider();
            TimeSpan endValue = _endValueProvider();

            if (StartAdjustment.HasValue)
            {
                startValue += StartAdjustment.Value;
            }

            if (EndAdjustment.HasValue)
            {
                endValue += EndAdjustment.Value;
            }

            var timeRangeChecker = new TimeRangeChecker();
            if (timeRangeChecker.IsTimeInRange(_timer.CurrentTime, startValue, endValue))
            {
                return ConditionState.Fulfilled;
            }
            
            return ConditionState.NotFulfilled;
        }
    }
}
