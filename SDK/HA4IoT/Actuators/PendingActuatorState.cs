using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public class PendingActuatorState
    {
        public IActuator Actuator { get; private set; }

        public ComponentState State { get; private set; }

        public IList<IHardwareParameter> Parameters { get; } = new List<IHardwareParameter>();

        public PendingActuatorState WithActuator(IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuator = actuator;
            return this;
        }

        public PendingActuatorState WithState(ComponentState state)
        {
            State = state;
            return this;
        }

        public PendingActuatorState WithParameter(IHardwareParameter parameter)
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