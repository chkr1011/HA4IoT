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
        private readonly List<PendingComponentCommand> _commands = new List<PendingComponentCommand>();
        private readonly List<PendingBinaryOutputState> _binaryOutputs = new List<PendingBinaryOutputState>();

        public StateMachineState(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public string Id { get; }

        public StateMachineState WithAction(Action<IHardwareParameter[]> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
            return this;
        }

        public StateMachineState WithAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(_ => action());
            return this;
        }

        public StateMachineState WithCommand(IComponent component, ICommand command)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (command == null) throw new ArgumentNullException(nameof(command));

            _commands.Add(new PendingComponentCommand { Component = component, Command = command });
            return this;
        }

        public StateMachineState WithBinaryOutput(IBinaryOutput output, BinaryState state)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _binaryOutputs.Add(new PendingBinaryOutputState { BinaryOutput = output, State = state });
            return this;
        }

        public void Activate(params IHardwareParameter[] parameters)
        {
            foreach (var binaryOutput in _binaryOutputs)
            {
                binaryOutput.Execute(WriteBinaryStateMode.NoCommit);
            }

            var isPartOfPartialUpdate = parameters.Any(p => p is IsPartOfPartialUpdateParameter);

            if (!isPartOfPartialUpdate)
            {
                foreach (var binaryOutput in _binaryOutputs)
                {
                    binaryOutput.Execute(WriteBinaryStateMode.Commit);
                }

                foreach (var pendingActuatorState in _commands)
                {
                    pendingActuatorState.Execute();
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
