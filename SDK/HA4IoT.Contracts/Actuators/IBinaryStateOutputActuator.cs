using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IBinaryStateOutputActuator
    {
        event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        string Id { get; }

        BinaryActuatorState State { get; }

        void SetState(BinaryActuatorState state, bool commit = true);

        void Toggle(bool commit = true);

        void TurnOff(bool commit = true);

        void TurnOn(bool commit = true);
    }
}
