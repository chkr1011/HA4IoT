using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.StateMachines
{
    public class StateMachineState : IStateMachineState
    {
        private readonly List<Action<IHardwareParameter[]>> _actions = new List<Action<IHardwareParameter[]>>(); 
        private readonly List<PendingActuatorState> _pendingActuatorStates = new List<PendingActuatorState>();
        private readonly List<Tuple<IBinaryOutput, BinaryState>> _pendingBinaryOutputStates = new List<Tuple<IBinaryOutput, BinaryState>>();

        public StateMachineState(ComponentState id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public ComponentState Id { get; }

        public StateMachineState WithAction(Action<IHardwareParameter[]> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
            return this;
        }

        public StateMachineState WithOutput(IBinaryOutput output, BinaryState state)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _pendingBinaryOutputStates.Add(new Tuple<IBinaryOutput, BinaryState>(output, state));
            return this;
        }

        public StateMachineState WithLowOutput(IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            return WithOutput(output, BinaryState.Low);
        }

        public StateMachineState WithHighOutput(IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            return WithOutput(output, BinaryState.High);
        }

        public StateMachineState WithActuator(IActuator actuator, ComponentState state)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _pendingActuatorStates.Add(new PendingActuatorState().WithActuator(actuator).WithState(state));
            return this;
        }

        public void Activate(params IHardwareParameter[] parameters)
        {
            foreach (var port in _pendingBinaryOutputStates)
            {
                port.Item1.Write(port.Item2, false);
            }
            
            if (!parameters.Any(p => p is IsPartOfPartialUpdateParameter))
            {
                foreach (var port in _pendingBinaryOutputStates)
                {
                    port.Item1.Write(port.Item2);
                }

                foreach (var pendingActuatorState in _pendingActuatorStates)
                {
                    pendingActuatorState.Apply();
                }
            }

            foreach (var action in _actions)
            {
                action(parameters);
            }
        }

        public void Deactivate(params IHardwareParameter[] parameters)
        {
        }
    }
}
