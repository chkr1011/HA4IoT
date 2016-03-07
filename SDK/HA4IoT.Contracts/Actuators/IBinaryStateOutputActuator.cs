using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IBinaryStateOutputActuator : IActuator, IActuatorWithOffState
    {
        event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        BinaryActuatorState GetState();

        void SetState(BinaryActuatorState state, params IParameter[] parameters);
    }
}
