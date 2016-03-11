using System;
using HA4IoT.Contracts.Actions;

namespace HA4IoT.Contracts.Actuators
{
    public interface IBinaryStateOutputActuator : IActuator, IActuatorWithOffState
    {
        event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        BinaryActuatorState GetState();

        void SetState(BinaryActuatorState state, params IParameter[] parameters);

        IActuatorAction GetTurnOnAction();

        IActuatorAction GetTurnOffAction();

        IActuatorAction GetToggleAction();
    }
}
