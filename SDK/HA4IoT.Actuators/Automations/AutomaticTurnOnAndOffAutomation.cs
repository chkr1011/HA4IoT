using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HA4IoT.Actuators.Conditions;
using HA4IoT.Actuators.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Actuators.Automations
{
    public class AutomaticTurnOnAndOffAutomation
    {
        private readonly ConditionsValidator _enablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _disablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);

        private readonly IHomeAutomationTimer _timer;

        private readonly List<Action> _turnOnActions = new List<Action>();
        private readonly List<Action> _turnOffActions = new List<Action>();
        
        private readonly Stopwatch _lastTurnedOn = new Stopwatch();

        private TimeSpan _duration;
        private TimeSpan? _pauseDuration;
        private TimedAction _turnOffTimeout;
        private bool _turnOffIfButtonPressedWhileAlreadyOn;
        private bool _isOn;
        
        public AutomaticTurnOnAndOffAutomation(IHomeAutomationTimer timer)
        {
            _timer = timer;

            WithOnDuration(TimeSpan.FromMinutes(1));
        }

        public AutomaticTurnOnAndOffAutomation WithTrigger(IMotionDetector motionDetector, params IParameter[] parameters)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            motionDetector.GetMotionDetectedTrigger().Triggered += (s, e) => Trigger();
            motionDetector.GetDetectionCompletedTrigger().Triggered += (s, e) => StartTimeout();
            motionDetector.IsEnabledChanged += CancelTimeoutIfMotionDetectorDeactivated;
            
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithTrigger(ITrigger trigger, params IParameter[] parameters)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            trigger.Triggered += HandleButtonPressed;
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithTurnOnAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _turnOnActions.Add(action);
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithTurnOffAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _turnOffActions.Add(action);
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithTarget(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _turnOnActions.Add(() => actuator.TurnOn());
            _turnOffActions.Add(() => actuator.TurnOff());
            
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

        public AutomaticTurnOnAndOffAutomation WithTurnOnWithinTimeRange(Func<TimeSpan> from, Func<TimeSpan> until)
        {
            if (@from == null) throw new ArgumentNullException(nameof(@from));
            if (until == null) throw new ArgumentNullException(nameof(until));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(from).WithEnd(until));
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithTurnOnIfAllRollerShuttersClosed(params IRollerShutter[] rollerShutters)
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

        public AutomaticTurnOnAndOffAutomation WithEnabledAtDay(IWeatherStation weatherStation)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            Func<TimeSpan> start = () => weatherStation.Daylight.Sunrise.Add(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => weatherStation.Daylight.Sunset.Subtract(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(start).WithEnd(end));
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithEnabledAtNight(IWeatherStation weatherStation)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            Func<TimeSpan> start = () => weatherStation.Daylight.Sunset.Subtract(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => weatherStation.Daylight.Sunrise.Add(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(start).WithEnd(end));
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithSkipIfAnyActuatorIsAlreadyOn(params IBinaryStateOutputActuator[] actuators)
        {
            if (actuators == null) throw new ArgumentNullException(nameof(actuators));

            _disablingConditionsValidator.WithCondition(ConditionRelation.Or,
                new Condition().WithExpression(() => actuators.Any(a => a.GetState() == BinaryActuatorState.On)));

            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithTurnOffIfButtonPressedWhileAlreadyOn()
        {
            _turnOffIfButtonPressedWhileAlreadyOn = true;
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithPauseAfterEveryTurnOn(TimeSpan duration)
        {
            _pauseDuration = duration;
            return this;
        }

        private void HandleButtonPressed(object sender, EventArgs e)
        {
            if (_turnOffIfButtonPressedWhileAlreadyOn && _isOn)
            {
                TurnOff();
                return;
            }

            // The state should be turned on because manual actions are not conditional.
            TurnOn();
            StartTimeout();
        }

        private void CancelTimeoutIfMotionDetectorDeactivated(object sender, ActuatorIsEnabledChangedEventArgs e)
        {
            bool isDeactivated = !e.NewValue;

            if (isDeactivated)
            {
                _turnOffTimeout?.Cancel();
            }
        }

        private void Trigger()
        {
            if (!GetConditionsAreFulfilled())
            {
                return;
            }

            if (IsPausing())
            {
                return;
            }

            TurnOn();
        }

        private bool IsPausing()
        {
            if (!_pauseDuration.HasValue)
            {
                return false;
            }

            if (_lastTurnedOn.Elapsed < _pauseDuration.Value)
            {
                return true;
            }

            return false;
        }

        private void TurnOn()
        {
            _turnOffTimeout?.Cancel();

            foreach (var action in _turnOnActions)
            {
                action();
            }
            
            _isOn = true;
            _lastTurnedOn.Restart();
        }

        private void TurnOff()
        {
            _turnOffTimeout?.Cancel();

            foreach (var action in _turnOffActions)
            {
                action();
            }

            _isOn = false;
            _lastTurnedOn.Stop();
        }

        private void StartTimeout()
        {
            if (!GetConditionsAreFulfilled())
            {
                return;
            }

            _turnOffTimeout = _timer.In(_duration).Do(TurnOff);
        }

        private bool GetConditionsAreFulfilled()
        {
            if (_disablingConditionsValidator.Conditions.Any() && _disablingConditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                return false;
            }

            if (_enablingConditionsValidator.Conditions.Any() && _enablingConditionsValidator.Validate() == ConditionState.NotFulfilled)
            {
                return false;
            }

            return true;
        }
    }
}
