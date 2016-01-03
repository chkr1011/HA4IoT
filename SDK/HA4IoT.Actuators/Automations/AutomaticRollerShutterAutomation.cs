using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators.Conditions.Specialized;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Automations
{
    public class AutomaticRollerShutterAutomation
    {
        private readonly List<IRollerShutter> _rollerShutters = new List<IRollerShutter>();
        private readonly IWeatherStation _weatherStation;
        private readonly INotificationHandler _notificationHandler;

        private bool _maxOutsideTemperatureApplied;

        private bool _autoOpenIsApplied;
        private bool _autoCloseIsApplied;
        private bool _doNotOpenBeforeIsTraced;
        private bool _doNotOpenIfTemperatureIsTraced;
        
        public AutomaticRollerShutterAutomation(IHomeAutomationTimer timer, IWeatherStation weatherStation, INotificationHandler notificationHandler)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            _weatherStation = weatherStation;
            _notificationHandler = notificationHandler;

            AutomaticallyOpenTimeRange = new IsDayCondition(weatherStation, timer);
            AutomaticallyOpenTimeRange.WithStartAdjustment(TimeSpan.FromMinutes(-30));
            AutomaticallyOpenTimeRange.WithEndAdjustment(TimeSpan.FromMinutes(30));

            IsEnabled = true;

            timer.Every(TimeSpan.FromSeconds(10)).Do(PerformPendingActions);
        }

        public TimeRangeCondition AutomaticallyOpenTimeRange { get; }

        public TimeSpan? DoNotOpenBefore { get; set; }

        public float? MaxOutsideTemperatureForAutoClose { get; private set; }

        public float? MinOutsideTemperatureForDoNotOpen { get; private set; }

        public bool IsEnabled { get; set; }

        public AutomaticRollerShutterAutomation WithRollerShutters(params IRollerShutter[] rollerShutters)
        {
            if (rollerShutters == null) throw new ArgumentNullException(nameof(rollerShutters));

            _rollerShutters.AddRange(rollerShutters);
            return this;
        }

        public AutomaticRollerShutterAutomation WithDoNotOpenBefore(TimeSpan minTime)
        {
            DoNotOpenBefore = minTime;
            return this;
        }

        public AutomaticRollerShutterAutomation WithDoNotOpenIfOutsideTemperatureIsBelowThan(float minOutsideTemperature)
        {
            MinOutsideTemperatureForDoNotOpen = minOutsideTemperature;
            return this;
        }

        public AutomaticRollerShutterAutomation WithCloseIfOutsideTemperatureIsGreaterThan(float maxOutsideTemperature)
        {
            MaxOutsideTemperatureForAutoClose = maxOutsideTemperature;
            return this;
        }

        private void PerformPendingActions()
        {
            if (!IsEnabled)
            {
                return;
            }

            if (MaxOutsideTemperatureForAutoClose.HasValue && !_maxOutsideTemperatureApplied)
            {
                if (_weatherStation.TemperatureSensor.GetValue() > MaxOutsideTemperatureForAutoClose.Value)
                {
                    _maxOutsideTemperatureApplied = true;
                    StartMove(RollerShutterState.MovingDown);

                    _notificationHandler.Info(GetTracePrefix() + "Closing because outside temperature reaches " + MaxOutsideTemperatureForAutoClose + "°C");

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
                if (DoNotOpenBefore.HasValue && DoNotOpenBefore.Value > time)
                {
                    if (!_doNotOpenBeforeIsTraced)
                    {
                        _notificationHandler.Info(GetTracePrefix() + "Skipping opening because it is too early.");
                        _doNotOpenBeforeIsTraced = true;
                    }
                    
                    return;
                }

                // Consider creating an object for conditional traces.
                _doNotOpenBeforeIsTraced = false;

                if (MinOutsideTemperatureForDoNotOpen.HasValue &&
                    _weatherStation.TemperatureSensor.GetValue() < MinOutsideTemperatureForDoNotOpen.Value)
                {
                    if (!_doNotOpenIfTemperatureIsTraced)
                    {
                        _notificationHandler.Info(GetTracePrefix() + "Skipping opening because it is too cold (" + MinOutsideTemperatureForDoNotOpen + "°C).");
                        _doNotOpenIfTemperatureIsTraced = true;
                    }

                    return;
                }

                _doNotOpenBeforeIsTraced = false;

                _autoOpenIsApplied = true;
                _autoCloseIsApplied = false;
                _maxOutsideTemperatureApplied = false;

                StartMove(RollerShutterState.MovingUp);
                _notificationHandler.Info(GetTracePrefix() + "Applied sunrise");

                return;
            }

            if (!_autoCloseIsApplied && autoCloseIsInRange)
            {
                _autoCloseIsApplied = true;
                _autoOpenIsApplied = false;
                
                StartMove(RollerShutterState.MovingDown);
                _notificationHandler.Info(GetTracePrefix() + "Applied sunset");
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
