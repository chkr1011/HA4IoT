using System;
using HA4IoT.Actuators.Parameters;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators
{
    public abstract class BinaryStateActuator : StateMachine
    {
        protected BinaryStateActuator(ActuatorId id, IBinaryStateEndpoint endpoint) 
            : base(id)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            AddState(new StateMachineState(DefaultStateIDs.Off, this).WithAction(() => endpoint.TurnOff()));
            AddState(new StateMachineState(DefaultStateIDs.On, this).WithAction(() => endpoint.TurnOn()));

            SetActiveState(DefaultStateIDs.Off, new ForceUpdateStateParameter());
        }
    }
}