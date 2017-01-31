using System;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public abstract class BinaryStateActuator : StateMachine
    {
        protected BinaryStateActuator(ComponentId id, IBinaryOutputComponentAdapter adapter) 
            : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            AddState(new StateMachineState(BinaryStateId.Off).WithAction(adapter.TurnOff));
            AddState(new StateMachineState(BinaryStateId.On).WithAction(adapter.TurnOn));
        }
    }
}