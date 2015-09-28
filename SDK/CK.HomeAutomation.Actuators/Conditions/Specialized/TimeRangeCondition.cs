using System;
using CK.HomeAutomation.Core.Timer;

namespace CK.HomeAutomation.Actuators.Conditions.Specialized
{
    public class TimeRangeCondition : Condition
    {
        private readonly IHomeAutomationTimer _timer;

        private Func<TimeSpan> _startValueProvider;
        private Func<TimeSpan> _endValueProvider;

        private TimeSpan? _startAdjustment;
        private TimeSpan? _endAdjustment;

        public TimeRangeCondition(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
            WithExpression(() => Check());
        }

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
            _startAdjustment = value;
            return this;
        }

        public TimeRangeCondition WithEndAdjustment(TimeSpan value)
        {
            _endAdjustment = value;
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

            if (_startAdjustment.HasValue)
            {
                startValue += _startAdjustment.Value;
            }

            if (_endAdjustment.HasValue)
            {
                endValue += _endAdjustment.Value;
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
