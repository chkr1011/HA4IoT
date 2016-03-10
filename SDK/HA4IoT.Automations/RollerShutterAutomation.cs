using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomation : AutomationBase<RollerShutterAutomationSettings>
    {
        private readonly List<ActuatorId> _rollerShutters = new List<ActuatorId>();

        private readonly IHomeAutomationTimer _timer;
        private readonly IWeatherStation _weatherStation;
        private readonly IActuatorController _actuatorController;
        private readonly ILogger _logger;

        private bool _maxOutsideTemperatureApplied;
        private bool _autoOpenIsApplied;
        private bool _autoCloseIsApplied;
        
        public RollerShutterAutomation(AutomationId id, IHomeAutomationTimer timer, IWeatherStation weatherStation, IHttpRequestController httpApiController, IActuatorController actuatorController, ILogger logger)
            : base(id)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));
            if (actuatorController == null) throw new ArgumentNullException(nameof(actuatorController));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _timer = timer;
            _weatherStation = weatherStation;
            _actuatorController = actuatorController;
            _logger = logger;

            Settings = new RollerShutterAutomationSettings(id, httpApiController, logger);           
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

                _logger.Info(GetTracePrefix() + $"Closing because outside temperature reaches {Settings.AutoCloseIfTooHotTemperaure.Value}°C.");
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
                    _logger.Info(GetTracePrefix() + $"Cancelling opening because outside temperature is lower than {Settings.DoNotOpenIfTooColdTemperature.Value}°C.");
                }
                else
                {
                    StartMove(RollerShutterState.MovingUp);
                }
                
                _autoOpenIsApplied = true;
                _autoCloseIsApplied = false;

                _maxOutsideTemperatureApplied = false;
                
                _logger.Info(GetTracePrefix() + "Applied sunrise");                
            }
            else if (!_autoCloseIsApplied && autoCloseIsInRange)
            {
                if (TooColdIsAffected())
                {
                    _logger.Info(GetTracePrefix() + $"Cancelling closing because outside temperature is lower than {Settings.DoNotOpenIfTooColdTemperature.Value}°C.");
                }
                else
                {
                    StartMove(RollerShutterState.MovingDown);
                }

                _autoCloseIsApplied = true;
                _autoOpenIsApplied = false;
                
                _logger.Info(GetTracePrefix() + "Applied sunset");
            }
        }

        private bool DoNotOpenDueToTimeIsAffected()
        {
            if (Settings.DoNotOpenBeforeIsEnabled.Value && 
                Settings.DoNotOpenBeforeTime.Value > _timer.CurrentTime)
            {
                return true;
            }

            return false;
        }

        private bool TooHotIsAffected()
        {
            if (Settings.AutoCloseIfTooHotIsEnabled.Value && 
                _weatherStation.TemperatureSensor.GetValue() > Settings.AutoCloseIfTooHotTemperaure.Value)
            {
                return true;
            }

            return false;
        }

        private bool TooColdIsAffected()
        {
            if (Settings.DoNotOpenIfTooColdIsEnabled.Value &&
                _weatherStation.TemperatureSensor.GetValue() < Settings.DoNotOpenIfTooColdTemperature.Value)
            {
                return true;
            }

            return false;
        }

        private IsDayCondition GetIsDayCondition()
        {
            var condition = new IsDayCondition(_weatherStation, _timer);
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
