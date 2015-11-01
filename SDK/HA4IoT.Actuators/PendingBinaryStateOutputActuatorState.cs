using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators
{
    public class PendingBinaryStateOutputActuatorState
    {
        public IBinaryStateOutputActuator Actuator { get; private set; }

        public BinaryActuatorState State { get; private set; }

        public IList<IParameter> Parameters { get; } = new List<IParameter>();

        public PendingBinaryStateOutputActuatorState WithActuator(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuator = actuator;
            return this;
        }

        public PendingBinaryStateOutputActuatorState WithState(BinaryActuatorState state)
        {
            State = state;
            return this;
        }

        public PendingBinaryStateOutputActuatorState WithParameter(IParameter parameter)
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