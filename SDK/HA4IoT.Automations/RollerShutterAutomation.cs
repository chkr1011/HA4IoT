using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Networking;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomation : AutomationBase<RollerShutterAutomationSettings>
    {
        private readonly List<IRollerShutter> _rollerShutters = new List<IRollerShutter>();
        private readonly IWeatherStation _weatherStation;
        private readonly ILogger _logger;

        private bool _maxOutsideTemperatureApplied;

        private bool _autoOpenIsApplied;
        private bool _autoCloseIsApplied;
        private bool _doNotOpenBeforeIsTraced; // TODO: Create trace wrapper with flag and "Reset()" method?
        private bool _doNotOpenIfTemperatureIsTraced;
        
        public RollerShutterAutomation(AutomationId id, IHomeAutomationTimer timer, IWeatherStation weatherStation, IHttpRequestController httpApiController, ILogger logger)
            : base(id)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _weatherStation = weatherStation;
            _logger = logger;

            Settings = new RollerShutterAutomationSettings(id, httpApiController, logger);

            AutomaticallyOpenTimeRange = new IsDayCondition(weatherStation, timer);
            AutomaticallyOpenTimeRange.WithStartAdjustment(TimeSpan.FromMinutes(-30));
            AutomaticallyOpenTimeRange.WithEndAdjustment(TimeSpan.FromMinutes(30));
            
            timer.Every(TimeSpan.FromSeconds(10)).Do(PerformPendingActions);
        }

        public TimeRangeCondition AutomaticallyOpenTimeRange { get; }

        public RollerShutterAutomation WithRollerShutters(params IRollerShutter[] rollerShutters)
        {
            if (rollerShutters == null) throw new ArgumentNullException(nameof(rollerShutters));

            _rollerShutters.AddRange(rollerShutters);
            return this;
        }

        public RollerShutterAutomation WithDoNotOpenBefore(TimeSpan minTime)
        {
            Settings.DoNotOpenBefore.Value = minTime;
            return this;
        }

        public RollerShutterAutomation WithDoNotOpenIfOutsideTemperatureIsBelowThan(float minOutsideTemperature)
        {
            Settings.MinOutsideTemperatureForDoNotOpen.Value = minOutsideTemperature;
            return this;
        }

        public RollerShutterAutomation WithCloseIfOutsideTemperatureIsGreaterThan(float maxOutsideTemperature)
        {
            Settings.MaxOutsideTemperatureForAutoClose.Value = maxOutsideTemperature;
            return this;
        }

        private void PerformPendingActions()
        {
            if (!Settings.IsEnabled.Value)
            {
                return;
            }

            if (Settings.MaxOutsideTemperatureForAutoClose.Value.HasValue && !_maxOutsideTemperatureApplied)
            {
                if (_weatherStation.TemperatureSensor.GetValue() > Settings.MaxOutsideTemperatureForAutoClose.Value)
                {
                    _maxOutsideTemperatureApplied = true;
                    StartMove(RollerShutterState.MovingDown);

                    _logger.Info(GetTracePrefix() + "Closing because outside temperature reaches " + Settings.MaxOutsideTemperatureForAutoClose + "°C");

                    return;
                }
            }

            Daylight daylightNow = _weatherStation.Daylight;

            bool daylightInformationIsAvailable = daylightNow.Sunrise != TimeSpan.Zero && daylightNow.Sunset != TimeSpan.Zero;
            if (!daylightInformationIsAvailable)
            {
                return;
            }


            bool autoOpenIsInRange = AutomaticallyOpenTimeRange.GetIsFulfilled();
            bool autoCloseIsInRange = !autoOpenIsInRange;

            if (!_autoOpenIsApplied && autoOpenIsInRange)
            {
                TimeSpan time = DateTime.Now.TimeOfDay;
                if (Settings.DoNotOpenBefore.Value.HasValue && Settings.DoNotOpenBefore.Value > time)
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

                if (Settings.MinOutsideTemperatureForDoNotOpen.Value.HasValue &&
                    _weatherStation.TemperatureSensor.GetValue() < Settings.MinOutsideTemperatureForDoNotOpen.Value)
                {
                    if (!_doNotOpenIfTemperatureIsTraced)
                    {
                        _logger.Info(GetTracePrefix() + "Skipping opening because it is too cold (" + Settings.MinOutsideTemperatureForDoNotOpen + "°C).");
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

        private void StartMove(RollerShutterState state)
        {
            foreach (var rollerShutter in _rollerShutters)
            {
                rollerShutter.SetState(state);
            }
        }

        private string GetTracePrefix()
        {
            return "Auto " + string.Join(",", _rollerShutters.Select(rs => rs.Id)) + ": ";
        }
    }
}
