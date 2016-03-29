using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IActuator
    {
        event EventHandler<StateMachineStateChangedEventArgs> ActiveStateChanged;

        bool GetSupportsState(StateMachineStateId id);

        StateMachineStateId GetActiveState();

        void SetActiveState(StateMachineStateId id, params IHardwareParameter[] parameters);

        StateMachineStateId GetNextState(StateMachineStateId id);

        void SetStateIdAlias(StateMachineStateId id, StateMachineStateId alias);
    }
}
