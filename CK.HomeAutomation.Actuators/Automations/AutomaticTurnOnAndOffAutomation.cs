using System;
using System.Linq;
using CK.HomeAutomation.Actuators.Conditions;
using CK.HomeAutomation.Actuators.Conditions.Specialized;
using CK.HomeAutomation.Core.Timer;

namespace CK.HomeAutomation.Actuators.Automations
{
    public class AutomaticTurnOnAndOffAutomation
    {
        private readonly ConditionsValidator _conditionValidator = new ConditionsValidator();
        private readonly IHomeAutomationTimer _timer;
        private IBinaryStateOutputActuator _actuator;
        private TimeSpan _duration;
        private TimedAction _turnOffTimeout;
        private bool _isOn;
        
        public AutomaticTurnOnAndOffAutomation(IHomeAutomationTimer timer)
        {
            _timer = timer;
        }

        public AutomaticTurnOnAndOffAutomation WithMotionDetector(MotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.MotionDetected += (s, e) => Trigger();
            motionDetector.DetectionCompleted += (s, e) => StartTimeout();

            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithButtonPressedShort(Button button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.PressedShort += (s, e) =>
            {
                Trigger();
                if (_isOn)
                {
                    StartTimeout();
                }
            };

            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithTarget(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuator = actuator;
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithOnDuration(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithOnWithinTimeRange(Func<TimeSpan> from, Func<TimeSpan> until)
        {
            if (@from == null) throw new ArgumentNullException(nameof(@from));
            if (until == null) throw new ArgumentNullException(nameof(until));

            _conditionValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(from).WithEnd(until));
            _conditionValidator.WithDefaultState(ConditionState.NotFulfilled);
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithOnIfAllRollerShuttersClosed(params RollerShutter[] rollerShutters)
        {
            if (rollerShutters == null) throw new ArgumentNullException(nameof(rollerShutters));

            var condition = new Condition().WithExpression(() => rollerShutters.First().IsClosed);
            foreach (var otherRollerShutter in rollerShutters.Skip(1))
            {
                condition.WithRelatedCondition(ConditionRelation.And, new Condition().WithExpression(() => otherRollerShutter.IsClosed));
            }

            _conditionValidator.WithCondition(ConditionRelation.Or, condition);
            _conditionValidator.WithDefaultState(ConditionState.NotFulfilled);
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithOnAtDayTimeRange(IWeatherStation weatherStation)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            Func<TimeSpan> start = () => weatherStation.Daylight.Sunrise.Add(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => weatherStation.Daylight.Sunset.Subtract(TimeSpan.FromHours(1));

            _conditionValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(start).WithEnd(end));
            _conditionValidator.WithDefaultState(ConditionState.NotFulfilled);
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithOnAtNightTimeRange(IWeatherStation weatherStation)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            Func<TimeSpan> start = () => weatherStation.Daylight.Sunset.Subtract(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => weatherStation.Daylight.Sunrise.Add(TimeSpan.FromHours(1));

            _conditionValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(start).WithEnd(end));
            _conditionValidator.WithDefaultState(ConditionState.NotFulfilled);
            return this;
        }

        private void Trigger()
        {
            if (_conditionValidator.Validate() == ConditionState.Fulfilled)
            {
                _isOn = true;

                _turnOffTimeout?.Cancel();
                _actuator.TurnOn();
            }
        }

        private void StartTimeout()
        {
            _turnOffTimeout?.Cancel();
            _turnOffTimeout = _timer.In(_duration).Do(() => _actuator.TurnOff());
            _isOn = false;
        }
    }
}
