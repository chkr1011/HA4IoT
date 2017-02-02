using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IActuator
    {
        GenericComponentState GetNextState(IComponentFeatureState baseStateId);

        void SetStateIdAlias(GenericComponentState id, GenericComponentState alias);
    }
}
