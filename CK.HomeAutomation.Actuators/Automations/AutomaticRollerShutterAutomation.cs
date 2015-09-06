using System;
using System.Collections.Generic;
using CK.HomeAutomation.Core;

namespace CK.HomeAutomation.Actuators.Automations
{
    public class AutomaticRollerShutterAutomation
    {
        private readonly IWeatherStation _weatherStation;
        private readonly List<RollerShutter> _rollerShutters = new List<RollerShutter>();

        private bool _sunriseApplied;
        private bool _sunsetApplied;
        private bool _maxOutsideTemperatureApplied;

        private float? _maxOutsideTemperature;

        public AutomaticRollerShutterAutomation(HomeAutomationTimer timer, IWeatherStation weatherStation)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

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

        public AutomaticRollerShutterAutomation WithRollerShutter(RollerShutter rollerShutter)
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
                if (_weatherStation.Temperature > _maxOutsideTemperature.Value)
                {
                    _maxOutsideTemperatureApplied = true;
                    StartMoveDown();

                    return;
                }
            }

            Daylight daylightNow = _weatherStation.Daylight;

            bool daylightInformationIsAvailable = daylightNow.Sunrise == TimeSpan.Zero || daylightNow.Sunset == TimeSpan.Zero;
            if (!daylightInformationIsAvailable)
            {
                return;
            }

            daylightNow = daylightNow.Move(SunriseDiff, SunsetDiff);

            TimeSpan time = DateTime.Now.TimeOfDay;
            if (time > daylightNow.Sunrise && time < daylightNow.Sunset)
            {
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
                rollerShutter.StartMoveUp();
            }
        }

        private void StartMoveDown()
        {
            foreach (var rollerShutter in _rollerShutters)
            {
                rollerShutter.StartMoveDown();
            }
        }

        public AutomaticRollerShutterAutomation WithDoNotOpenBefore(TimeSpan minTime)
        {
            DoNotOpenBefore = minTime;
            return this;
        }

        public AutomaticRollerShutterAutomation WithCloseIfOutsideTemperatureIsGreaterThan(float maxOutsideTemperature)
        {
            // TODO: Check!
            //_maxOutsideTemperature = maxOutsideTemperature;
            return this;
        }
    }
}
