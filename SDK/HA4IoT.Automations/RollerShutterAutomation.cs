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
        private bool _doNotOpenBeforeIsTraced; // TODO: Create trace wrapper with flag and "Reset()" method?

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

            timer.Every(TimeSpan.FromSeconds(30)).Do(PerformPendingActions);
        }

        public RollerShutterAutomation WithRollerShutters(params IRollerShutter[] rollerShutters)
        {
            if (rollerShutters == null) throw new ArgumentNullException(nameof(rollerShutters));

            _rollerShutters.AddRange(rollerShutters.Select(rs => rs.Id));
            return this;
        }

        public RollerShutterAutomation WithDoNotOpenBefore(TimeSpan minTime)
        {
            Settings.DoNotOpenBeforeIsEnabled.Value = true;
            Settings.DoNotOpenBeforeTime.Value = minTime;

            return this;
        }

        public RollerShutterAutomation WithDoNotOpenIfOutsideTemperatureIsBelowThan(float minOutsideTemperature)
        {
            Settings.DoNotOpenIfTooColdIsEnabled.Value = true;
            Settings.DoNotOpenIfTooColdTemperature.Value = minOutsideTemperature;

            return this;
        }

        public RollerShutterAutomation WithCloseIfOutsideTemperatureIsGreaterThan(float maxOutsideTemperature)
        {
            Settings.AutoCloseIfTooHotIsEnabled.Value = true;
            Settings.AutoCloseIfTooHotTemperaure.Value = maxOutsideTemperature;

            return this;
        }

        private void PerformPendingActions()
        {
            if (!Settings.IsEnabled.Value)
            {
                return;
            }

            if (Settings.AutoCloseIfTooHotIsEnabled.Value && !_maxOutsideTemperatureApplied)
            {
                if (_weatherStation.TemperatureSensor.GetValue() > Settings.AutoCloseIfTooHotTemperaure.Value)
                {
                    _maxOutsideTemperatureApplied = true;
                    StartMove(RollerShutterState.MovingDown);

                    _logger.Info(GetTracePrefix() + "Closing because outside temperature reaches " + Settings.AutoCloseIfTooHotTemperaure.Value + "°C");

                    return;
                }
            }

            // TODO: Add check for heavy hailing

            bool autoOpenIsInRange = GetIsDayCondition().GetIsFulfilled();
            bool autoCloseIsInRange = !autoOpenIsInRange;

            if (!_autoOpenIsApplied && autoOpenIsInRange)
            {
                if (GetDoNotOpenDueToTimeIsAffected())
                {
                    return;
                }

                if (GetDoNotOpenDueToColdTemperatureIsAffected())
                {
                    _logger.Info(GetTracePrefix() + "Canceling opening because the roller shutter is maybe frozen.");
                    _autoOpenIsApplied = true;
                }
                
                _autoOpenIsApplied = true;
                _autoCloseIsApplied = false;
                _maxOutsideTemperatureApplied = false;

                StartMove(RollerShutterState.MovingUp);
                _logger.Info(GetTracePrefix() + "Applied sunrise");                
            }
            else if (!_autoCloseIsApplied && autoCloseIsInRange)
            {
                _autoCloseIsApplied = true;
                _autoOpenIsApplied = false;

                StartMove(RollerShutterState.MovingDown);
                _logger.Info(GetTracePrefix() + "Applied sunset");
            }
        }

        private bool GetDoNotOpenDueToTimeIsAffected()
        {
            TimeSpan time = _timer.CurrentTime;
            if (Settings.DoNotOpenBeforeIsEnabled.Value && Settings.DoNotOpenBeforeTime.Value > time)
            {
                // TODO: Create "Resetable" trace message.
                if (!_doNotOpenBeforeIsTraced)
                {
                    _logger.Info(GetTracePrefix() + "Skipping opening because it is too early.");
                    _doNotOpenBeforeIsTraced = true;
                }

                return true;
            }

            _doNotOpenBeforeIsTraced = false;
            return false;
        }

        private bool GetDoNotOpenDueToColdTemperatureIsAffected()
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
                _actuatorController.Actuator<IRollerShutter>(rollerShutter).SetState(state);
            }
        }

        private string GetTracePrefix()
        {
            return "Auto " + string.Join(",", _rollerShutters) + ": ";
        }
    }
}
