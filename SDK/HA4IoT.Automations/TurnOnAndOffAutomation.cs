using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Automations
{
    public class TurnOnAndOffAutomation : AutomationBase
    {
        private readonly ConditionsValidator _enablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _disablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);

        private readonly IHomeAutomationTimer _timer;

        private readonly List<Action> _turnOnActions = new List<Action>();
        private readonly List<Action> _turnOffActions = new List<Action>();

        private readonly TurnOnAndOffAutomationSettingsWrapper _wrappedSettings;
        private readonly Stopwatch _lastTurnedOn = new Stopwatch();

        private TimeSpan? _pauseDuration;
        private TimedAction _turnOffTimeout;
        private bool _turnOffIfButtonPressedWhileAlreadyOn;
        private bool _isOn;
        
        public TurnOnAndOffAutomation(AutomationId id, IHomeAutomationTimer timer)
            : base(id)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
            _wrappedSettings = new TurnOnAndOffAutomationSettingsWrapper(Settings);
        }

        public TurnOnAndOffAutomation WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.GetMotionDetectedTrigger().Attach(Trigger);
            motionDetector.GetDetectionCompletedTrigger().Attach(StartTimeout);

            motionDetector.Settings.ValueChanged += CancelTimeoutIfMotionDetectorDeactivated;
            
            return this;
        }

        public TurnOnAndOffAutomation WithTrigger(ITrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Triggered += HandleButtonPressed;
            return this;
        }

        public TurnOnAndOffAutomation WithTurnOnAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _turnOnActions.Add(action);
            return this;
        }

        public TurnOnAndOffAutomation WithTurnOffAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _turnOffActions.Add(action);
            return this;
        }

        public TurnOnAndOffAutomation WithTarget(IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _turnOnActions.Add(() => actuator.SetState(BinaryStateId.On));
            _turnOffActions.Add(() => actuator.SetState(BinaryStateId.Off));
            
            return this;
        }

        public TurnOnAndOffAutomation WithOnDuration(TimeSpan duration)
        {
            _wrappedSettings.Duration = duration;
            return this;
        }

        public TurnOnAndOffAutomation WithAlwaysOn()
        {
            _enablingConditionsValidator.WithDefaultState(ConditionState.Fulfilled);
            return this;
        }

        public TurnOnAndOffAutomation WithTurnOnWithinTimeRange(Func<TimeSpan> from, Func<TimeSpan> until)
        {
            if (@from == null) throw new ArgumentNullException(nameof(@from));
            if (until == null) throw new ArgumentNullException(nameof(until));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(from).WithEnd(until));
            return this;
        }

        public TurnOnAndOffAutomation WithTurnOnIfAllRollerShuttersClosed(params IRollerShutter[] rollerShutters)
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

        public TurnOnAndOffAutomation WithEnabledAtDay(IDaylightService daylightService)
        {
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            Func<TimeSpan> start = () => daylightService.GetSunrise().Add(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => daylightService.GetSunset().Subtract(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(start).WithEnd(end));
            return this;
        }

        public TurnOnAndOffAutomation WithEnabledAtNight(IDaylightService daylightService)
        {
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            Func<TimeSpan> start = () => daylightService.GetSunset().Subtract(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => daylightService.GetSunrise().Add(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_timer).WithStart(start).WithEnd(end));
            return this;
        }

        public TurnOnAndOffAutomation WithSkipIfAnyActuatorIsAlreadyOn(params IStateMachine[] actuators)
        {
            if (actuators == null) throw new ArgumentNullException(nameof(actuators));

            _disablingConditionsValidator.WithCondition(ConditionRelation.Or,
                new Condition().WithExpression(() => actuators.Any(a => a.GetState().Equals(BinaryStateId.On))));

            return this;
        }

        public TurnOnAndOffAutomation WithTurnOffIfButtonPressedWhileAlreadyOn()
        {
            _turnOffIfButtonPressedWhileAlreadyOn = true;
            return this;
        }

        public TurnOnAndOffAutomation WithPauseAfterEveryTurnOn(TimeSpan duration)
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

        private void CancelTimeoutIfMotionDetectorDeactivated(object sender, SettingValueChangedEventArgs eventArgs)
        {
            if (eventArgs.SettingName != AutomationSettingsWrapper.IsEnabledName)
            {
                return;
            }

            bool isDeactivated = !eventArgs.NewValue.GetBoolean();

            if (isDeactivated)
            {
                _turnOffTimeout?.Cancel();
            }
        }

        private void Trigger()
        {
            if (!this.GetIsEnabled())
            {
                return;
            }

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

            _turnOffTimeout = _timer.In(_wrappedSettings.Duration).Do(TurnOff);
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
