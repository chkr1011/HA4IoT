using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.Animations
{
    public class Frame
    {
        private readonly List<PendingActuatorState> _states = new List<PendingActuatorState>();

        public TimeSpan StartTime { get; private set; }

        public bool IsApplied { get; private set; }

        public Frame WithStartTime(TimeSpan startTime)
        {
            StartTime = startTime;
            return this;
        }

        public Frame WithTargetState(IActuator actuator, ComponentState state)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _states.Add(new PendingActuatorState().WithActuator(actuator).WithState(state));
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
