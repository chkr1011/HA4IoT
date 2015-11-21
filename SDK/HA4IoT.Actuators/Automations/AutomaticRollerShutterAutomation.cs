using System;
using System.Collections.Generic;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Automations
{
    public class AutomaticRollerShutterAutomation
    {
        private readonly List<IRollerShutter> _rollerShutters = new List<IRollerShutter>();
        private readonly IHomeAutomationTimer _timer;
        private readonly IWeatherStation _weatherStation;

        private float? _maxOutsideTemperature;
        private bool _maxOutsideTemperatureApplied;

        private bool _sunriseApplied;
        private bool _sunsetApplied;

        public AutomaticRollerShutterAutomation(IHomeAutomationTimer timer, IWeatherStation weatherStation)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            _timer = timer;
            _weatherStation = weatherStation;
            
            SunriseDiff = TimeSpan.FromMinutes(-30);
            SunsetDiff = TimeSpan.FromMinutes(30);
            IsEnabled = true;

            timer.Every(TimeSpan.FromSeconds(10)).Do(Check);
        }

        public TimeSpan SunriseDiff { get; set; }

        public TimeSpan SunsetDiff { get; set; }

        public TimeSpan? DoNotOpenBefore { get; set; }

        public bool IsEnabled { get; set; }

        public AutomaticRollerShutterAutomation WithRollerShutter(IRollerShutter rollerShutter)
        {
            if (rollerShutter == null) throw new ArgumentNullException(nameof(rollerShutter));

            _rollerShutters.Add(rollerShutter);
            return this;
        }

        private void Check()
        {
            if (!IsEnabled)
            {
                return;
            }

            if (_maxOutsideTemperature.HasValue && !_maxOutsideTemperatureApplied)
            {
                if (_weatherStation.Temperature.Value > _maxOutsideTemperature.Value)
                {
                    _maxOutsideTemperatureApplied = true;
                    StartMoveDown();

                    return;
                }
            }

            Daylight daylightNow = _weatherStation.Daylight;

            bool daylightInformationIsAvailable = daylightNow.Sunrise != TimeSpan.Zero && daylightNow.Sunset != TimeSpan.Zero;
            if (!daylightInformationIsAvailable)
            {
                return;
            }

            daylightNow = daylightNow.Move(SunriseDiff, SunsetDiff);
            var timeRangeChecker = new TimeRangeChecker();
            if (timeRangeChecker.IsTimeInRange(_timer.CurrentTime, daylightNow.Sunrise, daylightNow.Sunset))
            {
                TimeSpan time = DateTime.Now.TimeOfDay;
                if (DoNotOpenBefore.HasValue && DoNotOpenBefore.Value > time)
                {
                    return;
                }

                if (!_sunriseApplied)
                {
                    _sunriseApplied = true;
                    _sunsetApplied = false;
                    _maxOutsideTemperatureApplied = false;

                    StartMoveUp();
                }
            }
            else
            {
                if (!_sunsetApplied)
                {
                    _sunriseApplied = false;
                    _sunsetApplied = true;

                    StartMoveDown();
                }
            }
        }

        private void StartMoveUp()
        {
            foreach (var rollerShutter in _rollerShutters)
            {
                rollerShutter.SetState(RollerShutterState.MovingUp);
            }
        }

        private void StartMoveDown()
        {
            foreach (var rollerShutter in _rollerShutters)
            {
                rollerShutter.SetState(RollerShutterState.MovingDown);
            }
        }

        public AutomaticRollerShutterAutomation WithDoNotOpenBefore(TimeSpan minTime)
        {
            DoNotOpenBefore = minTime;
            return this;
        }

        public AutomaticRollerShutterAutomation WithCloseIfOutsideTemperatureIsGreaterThan(float maxOutsideTemperature)
        {
            _maxOutsideTemperature = maxOutsideTemperature;
            return this;
        }
    }
}
