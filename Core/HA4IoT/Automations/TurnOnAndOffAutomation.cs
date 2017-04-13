using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HA4IoT.Components;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Conditions;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Automations
{
    public class TurnOnAndOffAutomation : AutomationBase
    {
        private readonly ISettingsService _settingsService;
        private readonly object _syncRoot = new object();
        private readonly List<IComponent> _components = new List<IComponent>();

        private readonly ConditionsValidator _enablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _disablingConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);
        private readonly ConditionsValidator _turnOffConditionsValidator = new ConditionsValidator().WithDefaultState(ConditionState.NotFulfilled);

        private readonly IDateTimeService _dateTimeService;
        private readonly ISchedulerService _schedulerService;
        private readonly IDaylightService _daylightService;

        private readonly Stopwatch _lastTurnedOn = new Stopwatch();

        private TimeSpan? _pauseDuration;
        private IDelayedAction _turnOffTimeout;
        private bool _turnOffIfButtonPressedWhileAlreadyOn;
        private bool _isOn;

        public TurnOnAndOffAutomation(string id, IDateTimeService dateTimeService, ISchedulerService schedulerService, ISettingsService settingsService, IDaylightService daylightService)
            : base(id)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));

            settingsService.CreateSettingsMonitor<TurnOnAndOffAutomationSettings>(this, s => Settings = s.NewSettings);
        }

        public TurnOnAndOffAutomationSettings Settings { get; private set; }

        public TurnOnAndOffAutomation WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.MotionDetectedTrigger.Attach(ExecuteAutoTrigger);
            motionDetector.MotionDetectionCompletedTrigger.Attach(StartTimeout);

            _settingsService.CreateSettingsMonitor<MotionDetectorSettings>(motionDetector, CancelTimeoutIfMotionDetectorDeactivated);

            return this;
        }

        public TurnOnAndOffAutomation WithTrigger(ITrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Attach(ExecuteManualTrigger);
            return this;
        }

        public TurnOnAndOffAutomation WithTarget(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _components.Add(component);
            return this;
        }

        public TurnOnAndOffAutomation WithTurnOnWithinTimeRange(Func<TimeSpan> from, Func<TimeSpan> until)
        {
            if (@from == null) throw new ArgumentNullException(nameof(from));
            if (until == null) throw new ArgumentNullException(nameof(until));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_dateTimeService).WithStart(from).WithEnd(until));
            return this;
        }

        public TurnOnAndOffAutomation WithEnablingCondition(ConditionRelation relation, ICondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            _enablingConditionsValidator.WithCondition(relation, condition);
            return this;
        }

        public TurnOnAndOffAutomation WithEnabledAtDay()
        {
            TimeSpan Start() => _daylightService.Sunrise.Add(TimeSpan.FromHours(1));
            TimeSpan End() => _daylightService.Sunset.Subtract(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_dateTimeService).WithStart(Start).WithEnd(End));
            return this;
        }

        public TurnOnAndOffAutomation WithEnabledAtNight()
        {
            TimeSpan Start() => _daylightService.Sunset.Subtract(TimeSpan.FromHours(1));
            TimeSpan End() => _daylightService.Sunrise.Add(TimeSpan.FromHours(1));

            _enablingConditionsValidator.WithCondition(ConditionRelation.Or, new TimeRangeCondition(_dateTimeService).WithStart(Start).WithEnd(End));
            return this;
        }

        public TurnOnAndOffAutomation WithSkipIfAnyIsAlreadyOn(params IComponent[] components)
        {
            if (components == null) throw new ArgumentNullException(nameof(components));

            _disablingConditionsValidator.WithCondition(ConditionRelation.Or,
                new Condition().WithExpression(() => components.Any(a => a.GetState().Has(PowerState.On))));

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

        private void CancelTimeoutIfMotionDetectorDeactivated(SettingsChangedEventArgs<MotionDetectorSettings> e)
        {
            if (!e.NewSettings.IsEnabled)
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
                _turnOffTimeout = _schedulerService.In(Settings.Duration, TurnOff);
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

            _components.ForEach(c => c.TryTurnOn());

            _isOn = true;
            _lastTurnedOn.Restart();
        }

        private void TurnOff()
        {
            _turnOffTimeout?.Cancel();
            if (GetTurnOffConditionsAreFulfilled())
            {

                _components.ForEach(c => c.TryTurnOff());

                _isOn = false;
                _lastTurnedOn.Stop();
            }
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

        public TurnOnAndOffAutomation WithTurnOffCondition(ConditionRelation relation, ICondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            _turnOffConditionsValidator.WithCondition(relation, condition);
            return this;
        }

        public TurnOnAndOffAutomation WithDisableTurnOffWhenBinaryStateEnabled(IBinaryInput input)
        {
            return WithTurnOffCondition(ConditionRelation.Or, new BinaryInputStateCondition(input, BinaryState.High));
        }

        private bool GetTurnOffConditionsAreFulfilled()
        {
            if (_turnOffConditionsValidator.Conditions.Any() && _turnOffConditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                return false;
            }

            return true;
        }
    }
}
