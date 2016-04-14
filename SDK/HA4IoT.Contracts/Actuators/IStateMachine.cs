using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IActuator
    {
        bool GetSupportsState(IComponentState id);

        IComponentState GetNextState(IComponentState id);

        void SetStateIdAlias(IComponentState id, IComponentState alias);
    }
}
