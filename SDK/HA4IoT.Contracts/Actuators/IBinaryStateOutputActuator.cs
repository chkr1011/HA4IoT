using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IBinaryStateOutputActuator : IActuatorWithOffState
    {
        event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        string Id { get; }

        BinaryActuatorState GetState();

        void SetState(BinaryActuatorState state, params IParameter[] parameters);
    }
}
