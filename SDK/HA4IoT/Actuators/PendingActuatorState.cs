using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public class PendingActuatorState
    {
        public IComponent Actuator { get; private set; }

        public GenericComponentState State { get; private set; }

        public IList<IHardwareParameter> Parameters { get; } = new List<IHardwareParameter>();

        public PendingActuatorState WithActuator(IComponent actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuator = actuator;
            return this;
        }

        public PendingActuatorState WithState(GenericComponentState state)
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
            // TODO: Fix!
            //Actuator.ChangeState(State, Parameters.ToArray());
        }
    }
}