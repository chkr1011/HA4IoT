using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public class StateMachineState
    {
        private readonly List<Tuple<IBinaryStateOutputActuator, BinaryActuatorState>> _actuators = new List<Tuple<IBinaryStateOutputActuator, BinaryActuatorState>>();
        private readonly List<Tuple<IBinaryOutput, BinaryState>> _outputs = new List<Tuple<IBinaryOutput, BinaryState>>();
        private readonly StateMachine _stateMachine;

        public StateMachineState(string id, StateMachine stateMachine)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            Id = id;
            _stateMachine = stateMachine;
        }

        public string Id { get; }

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

        public StateMachineState WithActuator(IBinaryStateOutputActuator actuator, BinaryActuatorState state)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuators.Add(new Tuple<IBinaryStateOutputActuator, BinaryActuatorState>(actuator, state));
            return this;
        }

        public StateMachineState ConnectApplyStateWith(IButton button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.GetPressedShortlyTrigger().Attach(() => _stateMachine.SetState(Id));
            return this;
        }

        internal void Apply(params IHardwareParameter[] parameters)
        {
            foreach (var port in _outputs)
            {
                port.Item1.Write(port.Item2, false);
            }

            foreach (var actuator in _actuators)
            {
                actuator.Item1.SetState(actuator.Item2, new IsPartOfPartialUpdateParameter());
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
        }
    }
}
