using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts;
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
        private bool _doNotOpenIfTemperatureIsTraced;
        
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
          
            timer.Every(TimeSpan.FromSeconds(10)).Do(PerformPendingActions);
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

            Daylight daylightNow = _weatherStation.Daylight;

            bool daylightInformationIsAvailable = daylightNow.Sunrise != TimeSpan.Zero && daylightNow.Sunset != TimeSpan.Zero;
            if (!daylightInformationIsAvailable)
            {
                return;
            }


            bool autoOpenIsInRange = GetIsDayCondition().GetIsFulfilled();
            bool autoCloseIsInRange = !autoOpenIsInRange;

            if (!_autoOpenIsApplied && autoOpenIsInRange)
            {
                TimeSpan time = DateTime.Now.TimeOfDay;
                if (Settings.DoNotOpenBeforeIsEnabled.Value && Settings.DoNotOpenBeforeTime.Value > time)
                {
                    if (!_doNotOpenBeforeIsTraced)
                    {
                        _logger.Info(GetTracePrefix() + "Skipping opening because it is too early.");
                        _doNotOpenBeforeIsTraced = true;
                    }
                    
                    return;
                }

                // Consider creating an object for conditional traces.
                _doNotOpenBeforeIsTraced = false;

                if (Settings.DoNotOpenIfTooColdIsEnabled.Value &&
                    _weatherStation.TemperatureSensor.GetValue() < Settings.DoNotOpenIfTooColdTemperature.Value)
                {
                    if (!_doNotOpenIfTemperatureIsTraced)
                    {
                        _logger.Info(GetTracePrefix() + "Skipping opening because it is too cold (" + Settings.DoNotOpenIfTooColdTemperature + "°C).");
                        _doNotOpenIfTemperatureIsTraced = true;
                    }

                    return;
                }

                _doNotOpenBeforeIsTraced = false;

                _autoOpenIsApplied = true;
                _autoCloseIsApplied = false;
                _maxOutsideTemperatureApplied = false;

                StartMove(RollerShutterState.MovingUp);
                _logger.Info(GetTracePrefix() + "Applied sunrise");

                return;
            }

            if (!_autoCloseIsApplied && autoCloseIsInRange)
            {
                _autoCloseIsApplied = true;
                _autoOpenIsApplied = false;
                
                StartMove(RollerShutterState.MovingDown);
                _logger.Info(GetTracePrefix() + "Applied sunset");
            }
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
