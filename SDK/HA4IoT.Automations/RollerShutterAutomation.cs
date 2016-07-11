using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomation : AutomationBase
    {
        private readonly List<ComponentId> _rollerShutters = new List<ComponentId>();

        private readonly ISchedulerService _schedulerService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IComponentController _componentController;

        private bool _maxOutsideTemperatureApplied;
        private bool _autoOpenIsApplied;
        private bool _autoCloseIsApplied;
        
        public RollerShutterAutomation(
            AutomationId id, 
            ISchedulerService schedulerService,
            IDateTimeService dateTimeService,
            IDaylightService daylightService,
            IOutdoorTemperatureService outdoorTemperatureService,
            IComponentController componentController)
            : base(id)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (outdoorTemperatureService == null) throw new ArgumentNullException(nameof(outdoorTemperatureService));
            if (componentController == null) throw new ArgumentNullException(nameof(componentController));

            _schedulerService = schedulerService;
            _dateTimeService = dateTimeService;
            _daylightService = daylightService;
            _outdoorTemperatureService = outdoorTemperatureService;
            _componentController = componentController;

            SpecialSettingsWrapper = new RollerShutterAutomationSettingsWrapper(Settings);           
        }

        public RollerShutterAutomationSettingsWrapper SpecialSettingsWrapper { get; }

        public RollerShutterAutomation WithRollerShutters(params IRollerShutter[] rollerShutters)
        {
            if (rollerShutters == null) throw new ArgumentNullException(nameof(rollerShutters));

            _rollerShutters.AddRange(rollerShutters.Select(rs => rs.Id));
            return this;
        }

        public void Activate()
        {
            _schedulerService.Every(TimeSpan.FromSeconds(10)).Do(PerformPendingActions);
        }

        public void PerformPendingActions()
        {
            if (!this.IsEnabled())
            {
                return;
            }

            if (!_maxOutsideTemperatureApplied && TooHotIsAffected())
            {
                _maxOutsideTemperatureApplied = true;

                Log.Info(GetTracePrefix() + $"Closing because outside temperature reaches {SpecialSettingsWrapper.AutoCloseIfTooHotTemperaure}°C.");
                SetStates(RollerShutterStateId.MovingDown);

                return;
            }

            // TODO: Add check for heavy hailing

            bool autoOpenIsInRange = GetIsDayCondition().IsFulfilled();
            bool autoCloseIsInRange = !autoOpenIsInRange;

            if (!_autoOpenIsApplied && autoOpenIsInRange)
            {
                if (DoNotOpenDueToTimeIsAffected())
                {
                    return;
                }

                if (TooColdIsAffected())
                {
                    Log.Info(GetTracePrefix() + $"Cancelling opening because outside temperature is lower than {SpecialSettingsWrapper.SkipIfFrozenTemperature}°C.");

                    _autoOpenIsApplied = true;
                    _autoCloseIsApplied = false;

                    return;
                }

                if (TooHotIsAffected())
                {
                    Log.Info(GetTracePrefix() + $"Cancelling opening because outside temperature is higher than {SpecialSettingsWrapper.AutoCloseIfTooHotTemperaure}°C.");

                    _autoOpenIsApplied = true;
                    _autoCloseIsApplied = false;
                    _maxOutsideTemperatureApplied = true;

                    return;
                }
                
                SetStates(RollerShutterStateId.MovingUp);

                _autoOpenIsApplied = true;
                _autoCloseIsApplied = false;

                _maxOutsideTemperatureApplied = false;
                
                Log.Info(GetTracePrefix() + "Applied sunrise");                
            }
            else if (!_autoCloseIsApplied && autoCloseIsInRange)
            {
                if (TooColdIsAffected())
                {
                    Log.Info(GetTracePrefix() + $"Cancelling closing because outside temperature is lower than {SpecialSettingsWrapper.SkipIfFrozenTemperature}°C.");
                }
                else
                {
                    SetStates(RollerShutterStateId.MovingDown);
                }

                _autoCloseIsApplied = true;
                _autoOpenIsApplied = false;
                
                Log.Info(GetTracePrefix() + "Applied sunset");
            }
        }

        private bool DoNotOpenDueToTimeIsAffected()
        {
            if (SpecialSettingsWrapper.SkipBeforeTimestampIsEnabled &&
                SpecialSettingsWrapper.SkipBeforeTimestamp > _dateTimeService.GetTime())
            {
                return true;
            }

            return false;
        }

        private bool TooHotIsAffected()
        {
            if (SpecialSettingsWrapper.AutoCloseIfTooHotIsEnabled && 
                _outdoorTemperatureService.GetOutdoorTemperature() > SpecialSettingsWrapper.AutoCloseIfTooHotTemperaure)
            {
                return true;
            }

            return false;
        }

        private bool TooColdIsAffected()
        {
            if (SpecialSettingsWrapper.SkipIfFrozenIsEnabled &&
                _outdoorTemperatureService.GetOutdoorTemperature() < SpecialSettingsWrapper.SkipIfFrozenTemperature)
            {
                return true;
            }

            return false;
        }

        private IsDayCondition GetIsDayCondition()
        {
            var condition = new IsDayCondition(_daylightService, _dateTimeService);
            condition.WithStartAdjustment(SpecialSettingsWrapper.OpenOnSunriseOffset);
            condition.WithEndAdjustment(SpecialSettingsWrapper.CloseOnSunsetOffset);

            return condition;
        }

        private void SetStates(NamedComponentState state)
        {
            foreach (var rollerShutter in _rollerShutters)
            {
                _componentController.GetComponent<IRollerShutter>(rollerShutter).SetState(state);
            }
        }

        private string GetTracePrefix()
        {
            return "Auto " + string.Join(",", _rollerShutters) + ": ";
        }
    }
}
