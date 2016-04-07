using System;
using HA4IoT.Actuators.Parameters;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public abstract class BinaryStateActuator : StateMachine
    {
        protected BinaryStateActuator(ComponentId id, IBinaryStateEndpoint endpoint) 
            : base(id)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            AddState(new StateMachineState(DefaultStateId.Off).WithAction(() => endpoint.TurnOff()));
            AddState(new StateMachineState(DefaultStateId.On).WithAction(() => endpoint.TurnOn()));

            SetActiveState(DefaultStateId.Off, new ForceUpdateStateParameter());
        }
    }
}