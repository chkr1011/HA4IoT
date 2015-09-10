using System;
using System.Collections.Generic;
using CK.HomeAutomation.Core.Timer;

namespace CK.HomeAutomation.Actuators.Automations
{
    public class AutomaticTurnOnAutomation
    {
        private readonly List<RollerShutter> _rollerShutters = new List<RollerShutter>();
        private readonly HomeAutomationTimer _timer;

        private IBinaryStateOutputActuator _actuator;
        private TimeSpan _duration = TimeSpan.FromMinutes(1);
        private Func<TimeSpan> _from;
        private TimedAction _timeout;
        private Func<TimeSpan> _until;

        public AutomaticTurnOnAutomation(HomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public AutomaticTurnOnAutomation WithMotionDetector(MotionDetector motionDetector)
        {
            motionDetector.MotionDetected += (s, e) => SetState(true, BinaryActuatorState.On);
            motionDetector.DetectionCompleted += (s, e) => StartTimeout();

            return this;
        }

        public AutomaticTurnOnAutomation WithTarget(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));
            if (_actuator != null)
            {
                throw new InvalidOperationException("Actuator is already set.");
            }

            _actuator = actuator;
            return this;
        }

        public AutomaticTurnOnAutomation WithButton(IButton button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.PressedShort += (s, e) =>
            {
                if (_actuator.State == BinaryActuatorState.Off)
                {
                    SetState(false, BinaryActuatorState.On);
                    StartTimeout();
                }
                else
                {
                    SetState(false, BinaryActuatorState.Off);
                    StopTimeout();
                }
            };

            return this;
        }

        public AutomaticTurnOnAutomation WithOnDuration(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }

        public AutomaticTurnOnAutomation WithEnabledInTimeRange(Func<TimeSpan> from, Func<TimeSpan> until)
        {
            if (@from == null) throw new ArgumentNullException(nameof(@from));
            if (until == null) throw new ArgumentNullException(nameof(until));

            _from = from;
            _until = until;

            return this;
        }

        public AutomaticTurnOnAutomation WithEnabledIfRollerShutterIsClosed(RollerShutter rollerShutter)
        {
            if (rollerShutter == null) throw new ArgumentNullException(nameof(rollerShutter));
            
            _rollerShutters.Add(rollerShutter);

            return this;
        }

        public AutomaticTurnOnAutomation WithOnlyAtDayTimeRange(IWeatherStation weatherStation)
        {
            _from = () => weatherStation.Daylight.Sunrise.Add(TimeSpan.FromHours(1));
            _until = () => weatherStation.Daylight.Sunset.Subtract(TimeSpan.FromHours(1));

            return this;
        }

        public AutomaticTurnOnAutomation WithOnlyAtNightTimeRange(IWeatherStation weatherStation)
        {
            _from = () => weatherStation.Daylight.Sunset.Subtract(TimeSpan.FromHours(1));
            _until = () => weatherStation.Daylight.Sunrise.Add(TimeSpan.FromHours(1));

            return this;
        }

        private void StartTimeout()
        {
            StopTimeout();
            _timeout = _timer.In(_duration).Do(() => _actuator.SetState(BinaryActuatorState.Off));
        }

        private void StopTimeout()
        {
            _timeout?.Cancel();
        }

        private void SetState(bool checkTimeRange, BinaryActuatorState state)
        {
            bool isEnabled = true;

            if (checkTimeRange)
            {
                if (!TimeRangeIsMatching())
                {
                    isEnabled = false;
                }
            }

            foreach (var rollerShutter in _rollerShutters)
            {
                if (rollerShutter.IsClosed)
                {
                    isEnabled = true;
                    break;
                }
            }

            if (isEnabled)
            {
                StopTimeout();
                _actuator?.SetState(state);
            }
        }

        private bool TimeRangeIsMatching()
        {
            if (_from == null || _until == null)
            {
                return true;
            }

            return new TimeRangeChecker().IsTimeInRange(_from(), _until());
        }
    }
}
