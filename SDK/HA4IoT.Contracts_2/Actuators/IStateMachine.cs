using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IActuator
    {
        ComponentState GetNextState(ComponentState baseStateId);

        void SetStateIdAlias(ComponentState id, ComponentState alias);
    }
}
