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
        private readonly List<Action> _actions = new List<Action>(); 
        private readonly List<Tuple<IStateMachine, StateId>> _actuators = new List<Tuple<IStateMachine, StateId>>();
        private readonly List<Tuple<IBinaryOutput, BinaryState>> _outputs = new List<Tuple<IBinaryOutput, BinaryState>>();

        public StateMachineState(IComponentState id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public IComponentState Id { get; }

        public StateMachineState WithAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
            return this;
        }

        public StateMachineState WithOutput(IBinaryOutput output, BinaryState state)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _outputs.Add(new Tuple<IBinaryOutput, BinaryState>(output, state));
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

        public StateMachineState WithActuator(IStateMachine actuator, StateId state)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuators.Add(new Tuple<IStateMachine, StateId>(actuator, state));
            return this;
        }

        public void Activate(params IHardwareParameter[] parameters)
        {
            foreach (var port in _outputs)
            {
                port.Item1.Write(port.Item2, false);
            }

            foreach (var actuator in _actuators)
            {
                actuator.Item1.SetState(actuator.Item2, HardwareParameter.IsPartOfPartialUpdate);
            }

            if (!parameters.Any(p => p is IsPartOfPartialUpdateParameter))
            {
                foreach (var port in _outputs)
                {
                    port.Item1.Write(port.Item2);
                }

                foreach (var actuator in _actuators)
                {
                    actuator.Item1.SetState(actuator.Item2);
                }
            }

            foreach (var action in _actions)
            {
                action();
            }
        }

        public void Deactivate(params IHardwareParameter[] parameters)
        {
        }
    }
}
