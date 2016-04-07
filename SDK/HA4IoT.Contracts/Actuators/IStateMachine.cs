using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IActuator
    {
        bool GetSupportsState(StateId id);

        void SetActiveState(StateId id, params IHardwareParameter[] parameters);

        StateId GetNextState(StateId id);

        void SetStateIdAlias(StateId id, StateId alias);
    }
}
