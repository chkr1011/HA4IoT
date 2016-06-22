using System;
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

            AddState(new StateMachineState(BinaryStateId.Off).WithAction(endpoint.TurnOff));
            AddState(new StateMachineState(BinaryStateId.On).WithAction(endpoint.TurnOn));
        }
    }
}