using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators.Conditions;
using HA4IoT.Actuators.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Automations
{
    public class AutomaticTurnOnAndOffAutomation
    {
        private readonly ConditionsValidator _enablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _disablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);

        private readonly IHomeAutomationTimer _timer;
        private readonly List<IBinaryStateOutputActuator> _actuators = new List<IBinaryStateOutputActuator>();

        private TimeSpan _duration;
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

            motionDetector.MotionDetected += (s, e) => Trigger();
            motionDetector.DetectionCompleted += (s, e) => StartTimeout();
            motionDetector.IsEnabledChanged += CancelTimeoutIfMotionDetectorDeactivated;
            
            return this;
        }

        public AutomaticTurnOnAndOffAutomation WithTrigger(IButton button, ButtonPressedDuration duration = ButtonPressedDuration.Short, params IParameter[] parameters)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            if (duration == ButtonPressedDuration.Short)
            {
                button.PressedShort += HandleButtonPressed;
            }
            else if (duration == ButtonPressedDuration.Long)
            {
                button.PressedLong += HandleButtonPressed;
            }
            else
            {
                throw new NotSupportedException();
            }
            
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

            TurnOn();
        }

        private void TurnOn()
        {
            _turnOffTimeout?.Cancel();
            _actuators.ForEach(a => a.TurnOn());

            _isOn = true;
        }

        private void TurnOff()
        {
            _turnOffTimeout?.Cancel();
            _actuators.ForEach(a => a.TurnOff());

            _isOn = false;
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
