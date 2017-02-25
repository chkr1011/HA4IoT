using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.StateMachines
{
    public class StateMachineState : IStateMachineState
    {
        private readonly List<Action<IHardwareParameter[]>> _actions = new List<Action<IHardwareParameter[]>>();
        private readonly List<PendingComponentCommand> _pendingComponentCommands = new List<PendingComponentCommand>();
        private readonly List<Tuple<IBinaryOutput, BinaryState>> _pendingBinaryOutputStates = new List<Tuple<IBinaryOutput, BinaryState>>();

        public StateMachineState(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public string Id { get; }

        public StateMachineState WithAction(Action<IHardwareParameter[]> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
            return this;
        }

        public StateMachineState WithCommand(IComponent component, ICommand command)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (command == null) throw new ArgumentNullException(nameof(command));

            _pendingComponentCommands.Add(new PendingComponentCommand { Component = component, Command = command });
            return this;
        }

        public StateMachineState WithBinaryOutput(IBinaryOutput output, BinaryState state)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _pendingBinaryOutputStates.Add(new Tuple<IBinaryOutput, BinaryState>(output, state));
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

                foreach (var pendingActuatorState in _pendingComponentCommands)
                {
                    pendingActuatorState.Invoke();
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
