using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Conditions;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Automations
{
    public class FlipFlopAutomation : AutomationBase
    {
        private readonly object _syncRoot = new object();
        private readonly ConditionsValidator _enablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _disablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);

        private readonly IDateTimeService _dateTimeService;
        private readonly ISchedulerService _schedulerService;
        private readonly IDaylightService _daylightService;

        private readonly List<Action> _flipActions = new List<Action>();
        private readonly List<Action> _flopActions = new List<Action>();

        private readonly Stopwatch _lastTurnedOn = new Stopwatch();

        private TimeSpan? _pauseDuration;
        private TimedAction _turnOffTimeout;
        private bool _turnOffIfButtonPressedWhileAlreadyOn;
        private bool _isOn;
        
        public FlipFlopAutomation(string id, IDateTimeService dateTimeService, ISchedulerService schedulerService, ISettingsService settingsService, IDaylightService daylightService)
            : base(id)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            _dateTimeService = dateTimeService;
            _schedulerService = schedulerService;
            _daylightService = daylightService;

            settingsService.CreateSettingsMonitor<TurnOnAndOffAutomationSettings>(Id, s => Settings = s);
        }

        public TurnOnAndOffAutomationSettings Settings { get; private set; }


        #region OLD MOVE SIMILAR TO EXTENSIONS.

        public FlipFlopAutomation WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.GetMotionDetectedTrigger().Attach(ExecuteAutoTrigger);
            motionDetector.GetDetectionCompletedTrigger().Attach(StartTimeout);

            motionDetector.Settings.ValueChanged += (s, e) => CancelTimeoutIfMotionDetectorDeactivated(motionDetector, e);

            return this;
        }

        #endregion

        public FlipFlopAutomation WithFlipTrigger(ITrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Attach(ExecuteManualTrigger);
            return this;
        }

        public FlipFlopAutomation WithFlipAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _flipActions.Add(action);
            return this;
        }

        public FlipFlopAutomation WithFlopAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _flopActions.Add(action);
            return this;
        }

        public FlipFlopAutomation WithTarget(IComponent actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            // TODO: Fix
            //_flipActions.Add(() => actuator.ChangeState(BinaryStateId.On));
            _flipActions.Add(() => actuator.InvokeCommand(new TurnOnCommand()));

            // TODO: Fix
            //_flopActions.Add(() => actuator.ChangeState(BinaryStateId.Off));
            _flopActions.Add(() => actuator.InvokeCommand(new TurnOffCommand()));

            return this;
        }

        public FlipFlopAutomation WithTurnOnWithinTimeRange(Func<TimeSpan> from, Func<TimeSpan> until)
        {
            if (@from == null) throw new ArgumentNullException(nameof(from));
            if (until == null) throw new ArgumentNullException(nameof(until));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_dateTimeService).WithStart(from).WithEnd(until));
            return this;
        }

        public FlipFlopAutomation WithEnablingCondition(ConditionRelation relation, ICondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            _enablingConditionsValidator.WithCondition(relation, condition);
            return this;
        }

        public FlipFlopAutomation WithEnabledAtDay()
        {
            Func<TimeSpan> start = () => _daylightService.Sunrise.Add(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => _daylightService.Sunset.Subtract(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_dateTimeService).WithStart(start).WithEnd(end));
            return this;
        }

        public FlipFlopAutomation WithEnabledAtNight()
        {
            Func<TimeSpan> start = () => _daylightService.Sunset.Subtract(TimeSpan.FromHours(1));
            Func<TimeSpan> end = () => _daylightService.Sunrise.Add(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_dateTimeService).WithStart(start).WithEnd(end));
            return this;
        }

        public FlipFlopAutomation WithSkipIfAnyActuatorIsAlreadyOn(params IComponent[] actuators)
        {
            if (actuators == null) throw new ArgumentNullException(nameof(actuators));

            _disablingConditionsValidator.WithCondition(ConditionRelation.Or,
                new Condition().WithExpression(() => actuators.Any(a => a.GetState().Has(PowerState.On))));

            return this;
        }

        public FlipFlopAutomation WithTurnOffIfButtonPressedWhileAlreadyOn()
        {
            _turnOffIfButtonPressedWhileAlreadyOn = true;
            return this;
        }

        public FlipFlopAutomation WithPauseAfterEveryTurnOn(TimeSpan duration)
        {
            _pauseDuration = duration;
            return this;
        }

        private void CancelTimeoutIfMotionDetectorDeactivated(IMotionDetector motionDetector, SettingValueChangedEventArgs eventArgs)
        {
            if (eventArgs.SettingName != "IsEnabled")
            {
                return;
            }
            
            if (!motionDetector.Settings.IsEnabled)
            {
                lock (_syncRoot)
                {
                    _turnOffTimeout?.Cancel();
                }
            }
        }

        private void ExecuteManualTrigger()
        {
            lock (_syncRoot)
            {
                if (_isOn)
                {
                    if (_turnOffIfButtonPressedWhileAlreadyOn)
                    {
                        TurnOff();
                        return;
                    }

                    StartTimeout();
                }
                else
                {
                    TurnOn();
                    StartTimeout();
                }
            }
        }

        private void ExecuteAutoTrigger()
        {
            lock (_syncRoot)
            {
                if (!Settings.IsEnabled)
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
        }

        private void StartTimeout()
        {
            lock (_syncRoot)
            {
                if (!GetConditionsAreFulfilled())
                {
                    return;
                }

                _turnOffTimeout?.Cancel();
                _turnOffTimeout = _schedulerService.In(Settings.Duration).Execute(TurnOff);
            }
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

            foreach (var action in _flipActions)
            {
                action();
            }
            
            _isOn = true;
            _lastTurnedOn.Restart();
        }

        private void TurnOff()
        {
            _turnOffTimeout?.Cancel();

            foreach (var action in _flopActions)
            {
                action();
            }

            _isOn = false;
            _lastTurnedOn.Stop();
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
