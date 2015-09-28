using System;
using System.Collections.Generic;
using System.Linq;
using CK.HomeAutomation.Actuators.Conditions;
using CK.HomeAutomation.Actuators.Conditions.Specialized;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Core.Timer;
using CK.HomeAutomation.Hardware;

namespace CK.HomeAutomation.Actuators.Automations
{
    public class AutomaticTurnOnAndOffAutomation
    {
        private readonly ConditionsValidator _enablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _disablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);

        private readonly IHomeAutomationTimer _timer;
        private readonly List<IBinaryStateOutputActuator> _actuators = new List<IBinaryStateOutputActuator>();
        private TimeSpan _duration;
        private TimedAction _turnOffTimeout;

        public AutomaticTurnOnAndOffAutomation(IHomeAutomationTimer timer)
        {
            _timer = timer;

            WithOnDuration(TimeSpan.FromMinutes(1));
        }

        public AutomaticTurnOnAndOffAutomation WithMotionDetector(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.MotionDetected += (s, e) => Trigger();
            motionDetector.DetectionCompleted += (s, e) => StartTimeout();

            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithButtonPressedShort(IButton button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.PressedShort += (s, e) =>
            {
                // The state should be turned on because manual actions are not conditional.
                TurnOn();
                StartTimeout();
            };

            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithTarget(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuators.Add(actuator);
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithOnDuration(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithAlwaysOn()
        {
            _enablingConditionsValidator.WithDefaultState(ConditionState.Fulfilled);
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithOnWithinTimeRange(Func<TimeSpan> from, Func<TimeSpan> until)
        {
            if (@from == null) throw new ArgumentNullException(nameof(@from));
            if (until == null) throw new ArgumentNullException(nameof(until));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(from).WithEnd(until));
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

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, condition);
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithOnAtDayTimeRange(IWeatherStation weatherStation)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            Func<TimeSpan> start = () => weatherStation.Daylight.Sunrise.Add(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => weatherStation.Daylight.Sunset.Subtract(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(start).WithEnd(end));
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithOnAtNightTimeRange(IWeatherStation weatherStation)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            Func<TimeSpan> start = () => weatherStation.Daylight.Sunset.Subtract(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => weatherStation.Daylight.Sunrise.Add(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(start).WithEnd(end));
            return this;
        }

        public void WithSkipIfAnyActuatorIsAlreadyOn(params IBinaryStateOutputActuator[] actuators)
        {
            if (actuators == null) throw new ArgumentNullException(nameof(actuators));

            _disablingConditionsValidator.WithCondition(ConditionRelation.Or,
                new Condition().WithExpression(() => actuators.Any(a => a.State == BinaryActuatorState.On)));
        }
        
        private void Trigger()
        {
            if (_disablingConditionsValidator.Conditions.Any() && _disablingConditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                return;
            }

            if (_enablingConditionsValidator.Conditions.Any() && _enablingConditionsValidator.Validate() == ConditionState.NotFulfilled)
            {
                return;
            }

            TurnOn();
        }

        private void TurnOn()
        {
            _turnOffTimeout?.Cancel();
            _actuators.ForEach(a => a.TurnOn());
        }

        private void StartTimeout()
        {
            _turnOffTimeout?.Cancel();
            _turnOffTimeout = _timer.In(_duration).Do(() =>
            {
                _actuators.ForEach(a => a.TurnOff());
            });
        }
    }
}
