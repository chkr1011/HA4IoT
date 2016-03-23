using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.WeatherService;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomation : AutomationBase<RollerShutterAutomationSettings>
    {
        private readonly List<ActuatorId> _rollerShutters = new List<ActuatorId>();

        private readonly IHomeAutomationTimer _timer;
        private readonly IDaylightService _daylightService;
        private readonly IWeatherService _weatherService;
        private readonly IActuatorController _actuatorController;

        private bool _maxOutsideTemperatureApplied;
        private bool _autoOpenIsApplied;
        private bool _autoCloseIsApplied;
        
        public RollerShutterAutomation(
            AutomationId id, 
            IHomeAutomationTimer timer,
            IDaylightService daylightService,
            IWeatherService weatherService,
            IApiController apiController,
            IActuatorController actuatorController)
            : base(id)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (weatherService == null) throw new ArgumentNullException(nameof(weatherService));
            if (actuatorController == null) throw new ArgumentNullException(nameof(actuatorController));

            _timer = timer;
            _daylightService = daylightService;
            _weatherService = weatherService;
            _actuatorController = actuatorController;

            Settings = new RollerShutterAutomationSettings(id, apiController);           
        }

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
            if (!Settings.IsEnabled.Value)
            {
                return;
            }

            if (!_maxOutsideTemperatureApplied && TooHotIsAffected())
            {
                _maxOutsideTemperatureApplied = true;

                Log.Info(GetTracePrefix() + $"Closing because outside temperature reaches {Settings.AutoCloseIfTooHotTemperaure.Value}°C.");
                StartMove(RollerShutterState.MovingDown);

                return;
            }

            // TODO: Add check for heavy hailing

            bool autoOpenIsInRange = GetIsDayCondition().GetIsFulfilled();
            bool autoCloseIsInRange = !autoOpenIsInRange;

            if (!_autoOpenIsApplied && autoOpenIsInRange)
            {
                if (DoNotOpenDueToTimeIsAffected())
                {
                    return;
                }

                if (TooColdIsAffected())
                {
                    Log.Info(GetTracePrefix() + $"Cancelling opening because outside temperature is lower than {Settings.SkipIfFrozenTemperature.Value}°C.");
                }
                else
                {
                    StartMove(RollerShutterState.MovingUp);
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
                    Log.Info(GetTracePrefix() + $"Cancelling closing because outside temperature is lower than {Settings.SkipIfFrozenTemperature.Value}°C.");
                }
                else
                {
                    StartMove(RollerShutterState.MovingDown);
                }

                _autoCloseIsApplied = true;
                _autoOpenIsApplied = false;
                
                Log.Info(GetTracePrefix() + "Applied sunset");
            }
        }

        private bool DoNotOpenDueToTimeIsAffected()
        {
            if (Settings.SkipBeforeTimestampIsEnabled.Value && 
                Settings.SkipBeforeTimestamp.Value > _timer.CurrentTime)
            {
                return true;
            }

            return false;
        }

        private bool TooHotIsAffected()
        {
            if (Settings.AutoCloseIfTooHotIsEnabled.Value && 
                _weatherService.TemperatureSensor.GetValue() > Settings.AutoCloseIfTooHotTemperaure.Value)
            {
                return true;
            }

            return false;
        }

        private bool TooColdIsAffected()
        {
            if (Settings.SkipIfFrozenIsEnabled.Value &&
                _weatherService.TemperatureSensor.GetValue() < Settings.SkipIfFrozenTemperature.Value)
            {
                return true;
            }

            return false;
        }

        private IsDayCondition GetIsDayCondition()
        {
            var condition = new IsDayCondition(_daylightService, _timer);
            condition.WithStartAdjustment(Settings.OpenOnSunriseOffset.Value);
            condition.WithEndAdjustment(Settings.CloseOnSunsetOffset.Value);

            return condition;
        }

        private void StartMove(RollerShutterState state)
        {
            foreach (var rollerShutter in _rollerShutters)
            {
                _actuatorController.GetActuator<IRollerShutter>(rollerShutter).SetState(state);
            }
        }

        private string GetTracePrefix()
        {
            return "Auto " + string.Join(",", _rollerShutters) + ": ";
        }
    }
}
