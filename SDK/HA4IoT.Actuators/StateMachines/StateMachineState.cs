using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators.Parameters;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Actuators
{
    public class StateMachineState : IStateMachineState
    {
        private readonly List<Action> _actions = new List<Action>(); 
        private readonly List<Tuple<IStateMachine, StateMachineStateId>> _actuators = new List<Tuple<IStateMachine, StateMachineStateId>>();
        private readonly List<Tuple<IBinaryOutput, BinaryState>> _outputs = new List<Tuple<IBinaryOutput, BinaryState>>();
        private readonly IStateMachine _stateMachine;

        public StateMachineState(StateMachineStateId id, IStateMachine stateMachine)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            Id = id;
            _stateMachine = stateMachine;
        }

        public StateMachineStateId Id { get; }

        public StateMachineState WithAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
            return this;
        }

        public StateMachineState WithPort(IBinaryOutput output, BinaryState state)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _outputs.Add(new Tuple<IBinaryOutput, BinaryState>(output, state));
            return this;
        }

        public StateMachineState WithLowPort(IBinaryOutput output)
        {
            return WithPort(output, BinaryState.Low);
        }

        public StateMachineState WithHighPort(IBinaryOutput output)
        {
            return WithPort(output, BinaryState.High);
        }

        public StateMachineState WithActuator(IStateMachine actuator, StateMachineStateId state)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuators.Add(new Tuple<IStateMachine, StateMachineStateId>(actuator, state));
            return this;
        }

        public StateMachineState ConnectApplyStateWith(IButton button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.GetPressedShortlyTrigger().Attach(() => _stateMachine.SetActiveState(Id));
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
                actuator.Item1.SetActiveState(actuator.Item2, new IsPartOfPartialUpdateParameter());
            }

            if (!parameters.Any(p => p is IsPartOfPartialUpdateParameter))
            {
                foreach (var port in _outputs)
                {
                    port.Item1.Write(port.Item2);
                }

                foreach (var actuator in _actuators)
                {
                    actuator.Item1.SetActiveState(actuator.Item2);
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
