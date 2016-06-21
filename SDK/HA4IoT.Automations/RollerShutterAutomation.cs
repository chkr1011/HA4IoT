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
using HA4IoT.Contracts.Services.WeatherService;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomation : AutomationBase
    {
        private readonly List<ComponentId> _rollerShutters = new List<ComponentId>();

        private readonly IHomeAutomationTimer _timer;
        private readonly IDaylightService _daylightService;
        private readonly IWeatherService _weatherService;
        private readonly IComponentController _componentController;

        private bool _maxOutsideTemperatureApplied;
        private bool _autoOpenIsApplied;
        private bool _autoCloseIsApplied;
        
        public RollerShutterAutomation(
            AutomationId id, 
            IHomeAutomationTimer timer,
            IDaylightService daylightService,
            IWeatherService weatherService,
            IComponentController componentController)
            : base(id)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (weatherService == null) throw new ArgumentNullException(nameof(weatherService));
            if (componentController == null) throw new ArgumentNullException(nameof(componentController));

            _timer = timer;
            _daylightService = daylightService;
            _weatherService = weatherService;
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
            _timer.Every(TimeSpan.FromSeconds(10)).Do(PerformPendingActions);
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
                }
                else
                {
                    SetStates(RollerShutterStateId.MovingUp);
                }
                
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
                SpecialSettingsWrapper.SkipBeforeTimestamp > _timer.CurrentTime)
            {
                return true;
            }

            return false;
        }

        private bool TooHotIsAffected()
        {
            if (SpecialSettingsWrapper.AutoCloseIfTooHotIsEnabled && 
                _weatherService.GetTemperature() > SpecialSettingsWrapper.AutoCloseIfTooHotTemperaure)
            {
                return true;
            }

            return false;
        }

        private bool TooColdIsAffected()
        {
            if (SpecialSettingsWrapper.SkipIfFrozenIsEnabled &&
                _weatherService.GetTemperature() < SpecialSettingsWrapper.SkipIfFrozenTemperature)
            {
                return true;
            }

            return false;
        }

        private IsDayCondition GetIsDayCondition()
        {
            var condition = new IsDayCondition(_daylightService, _timer);
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
