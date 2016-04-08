using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.Animations
{
    public class PendingBinaryStateOutputActuatorState
    {
        public IStateMachine Actuator { get; private set; }

        public StateId State { get; private set; }

        public IList<IHardwareParameter> Parameters { get; } = new List<IHardwareParameter>();

        public PendingBinaryStateOutputActuatorState WithActuator(IStateMachine actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuator = actuator;
            return this;
        }

        public PendingBinaryStateOutputActuatorState WithState(StateId state)
        {
            State = state;
            return this;
        }

        public PendingBinaryStateOutputActuatorState WithParameter(IHardwareParameter parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            Parameters.Add(parameter);
            return this;
        }

        public void Apply()
        {
            Actuator.SetState(State, Parameters.ToArray());
        }
    }
}