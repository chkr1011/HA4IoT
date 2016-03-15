using System;
using HA4IoT.Contracts.Actions;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IActuator, IActuatorWithOffState
    {
        event EventHandler<StateMachineStateChangedEventArgs> StateChanged;

        bool HasOffState { get; }

        void SetState(string id, params IHardwareParameter[] parameters);

        void SetNextState(params IHardwareParameter[] parameters);

        string GetState();

        IHomeAutomationAction GetSetNextStateAction();

        IHomeAutomationAction GetSetStateAction(string state);
    }
}
