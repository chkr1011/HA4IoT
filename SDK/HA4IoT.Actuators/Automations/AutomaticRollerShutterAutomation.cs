using System;
using System.Collections.Generic;
using HA4IoT.Actuators.Conditions.Specialized;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Core.Timer;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators.Automations
{
    public class AutomaticRollerShutterAutomation
    {
        private readonly List<IRollerShutter> _rollerShutters = new List<IRollerShutter>();
        private readonly IWeatherStation _weatherStation;
        private readonly INotificationHandler _notificationHandler;

        private bool _maxOutsideTemperatureApplied;

        private bool _sunriseApplied;
        private bool _sunsetApplied;

        public AutomaticRollerShutterAutomation(IHomeAutomationTimer timer, IWeatherStation weatherStation, INotificationHandler notificationHandler)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            _weatherStation = weatherStation;
            _notificationHandler = notificationHandler;

            SunriseCondition = new IsDayCondition(weatherStation, timer);
            SunriseCondition.WithStartAdjustment(TimeSpan.FromMinutes(-30));

            SunsetCondition = new IsNightCondition(weatherStation, timer);
            SunsetCondition.WithStartAdjustment(TimeSpan.FromMinutes(30));

            IsEnabled = true;

            timer.Every(TimeSpan.FromSeconds(10)).Do(PerformPendingActions);
        }

        public TimeRangeCondition SunriseCondition { get; }

        public TimeRangeCondition SunsetCondition { get; }

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
                if (_weatherStation.Temperature.Value > MaxOutsideTemperatureForAutoClose.Value)
                {
                    _maxOutsideTemperatureApplied = true;
                    StartMove(RollerShutterState.MovingDown);

                    _notificationHandler.Info("Closing because outside temperature reaches " + MaxOutsideTemperatureForAutoClose + "°C");

                    return;
                }
            }

            Daylight daylightNow = _weatherStation.Daylight;

            bool daylightInformationIsAvailable = daylightNow.Sunrise != TimeSpan.Zero && daylightNow.Sunset != TimeSpan.Zero;
            if (!daylightInformationIsAvailable)
            {
                return;
            }

            if (!_sunriseApplied && SunriseCondition.GetIsFulfilled())
            {
                TimeSpan time = DateTime.Now.TimeOfDay;
                if (DoNotOpenBefore.HasValue && DoNotOpenBefore.Value > time)
                {
                    // TODO: Trace!
                    return;
                }

                if (MinOutsideTemperatureForDoNotOpen.HasValue &&
                    _weatherStation.Temperature.Value < MinOutsideTemperatureForDoNotOpen.Value)
                {
                    // TODO: Trace!
                    return;
                }

                _sunriseApplied = true;
                _sunsetApplied = false;
                _maxOutsideTemperatureApplied = false;

                StartMove(RollerShutterState.MovingUp);
                _notificationHandler.Info("Applied sunrise");

                return;
            }

            if (!_sunsetApplied && SunsetCondition.GetIsFulfilled())
            {
                _sunriseApplied = false;
                _sunsetApplied = true;

                StartMove(RollerShutterState.MovingDown);
                _notificationHandler.Info("Applied sunset");
            }
        }

        private void StartMove(RollerShutterState state)
        {
            foreach (var rollerShutter in _rollerShutters)
            {
                rollerShutter.SetState(state);
            }
        }
    }
}
