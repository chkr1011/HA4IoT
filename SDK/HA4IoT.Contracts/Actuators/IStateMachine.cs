using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IActuator
    {
        IComponentState GetNextState(IComponentState baseStateId);

        void SetStateIdAlias(IComponentState id, IComponentState alias);
    }
}
