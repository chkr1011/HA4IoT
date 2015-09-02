using System;
using System.Collections.Generic;
using System.Linq;
using CK.HomeAutomation.Core;

namespace CK.HomeAutomation.Actuators.Automations
{
    public class AlwaysOnAutomation
    {
        private readonly List<IBinaryStateOutputActuator> _actuators = new List<IBinaryStateOutputActuator>();
        private readonly List<Tuple<TimeSpan, TimeSpan>> _offTimeRanges = new List<Tuple<TimeSpan, TimeSpan>>(); 

        private Func<TimeSpan> _from;
        private Func<TimeSpan> _until;

        public AlwaysOnAutomation(HomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            timer.Every(TimeSpan.FromMinutes(1)).Do(Update);
        }

        public AlwaysOnAutomation WithActuator(IBinaryStateOutputActuator actuator)
        {
            _actuators.Add(actuator);
            return this;
        }

        public AlwaysOnAutomation WithRange(Func<TimeSpan> from, Func<TimeSpan> until)
        {
            if (@from == null) throw new ArgumentNullException(nameof(@from));
            if (until == null) throw new ArgumentNullException(nameof(until));

            _from = from;
            _until = until;

            return this;
        }

        public AlwaysOnAutomation WithOnlyAtNightRange(IWeatherStation weatherStation)
        {
            return WithRange(() => weatherStation.Daylight.Sunset, () => weatherStation.Daylight.Sunrise);
        }

        public AlwaysOnAutomation WithOffBetweenRange(TimeSpan from, TimeSpan until)
        {
            _offTimeRanges.Add(new Tuple<TimeSpan, TimeSpan>(from, until));
            return this;
        }

        private void Update()
        {
            if (_from == null || _until == null)
            {
                return;
            }

            var timeRangeChecker = new TimeRangeChecker();

            if (_offTimeRanges.Any(offTimeRange => timeRangeChecker.IsTimeInRange(offTimeRange.Item1, offTimeRange.Item2)))
            {
                UpdateStates(BinaryActuatorState.Off);
                return;
            }

            TimeSpan from = _from();
            TimeSpan until = _until();

            bool isInOnRange = timeRangeChecker.IsTimeInRange(from, until);
            UpdateStates(isInOnRange ? BinaryActuatorState.On : BinaryActuatorState.Off);
        }

        private void UpdateStates(BinaryActuatorState state)
        {
            foreach (var actuator in _actuators)
            {
                actuator.SetState(state, false);
            }

            foreach (var actuator in _actuators)
            {
                actuator.SetState(state);
            }
        }
    }
}
