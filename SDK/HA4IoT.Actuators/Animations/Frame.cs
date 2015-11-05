using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators.Animations
{
    public class Frame
    {
        private readonly List<PendingBinaryStateOutputActuatorState> _states = new List<PendingBinaryStateOutputActuatorState>();

        public TimeSpan StartTime { get; private set; }

        public bool IsApplied { get; private set; }

        public Frame WithStartTime(TimeSpan startTime)
        {
            StartTime = startTime;
            return this;
        }

        public Frame WithTargetState(IBinaryStateOutputActuator actuator, BinaryActuatorState state)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _states.Add(new PendingBinaryStateOutputActuatorState().WithActuator(actuator).WithState(state));
            return this;
        }

        public void Apply()
        {
            foreach (var state in _states)
            {
                state.Apply();
            }

            IsApplied = true;
        }
    }
}
