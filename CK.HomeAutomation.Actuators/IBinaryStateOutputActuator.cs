using System;

namespace CK.HomeAutomation.Actuators
{
    public interface IBinaryStateOutputActuator
    {
        string Id { get; }

        BinaryActuatorState State { get; }
        event EventHandler StateChanged;

        void SetState(BinaryActuatorState state, bool commit = true);

        void Toggle(bool commit = true);

        void TurnOff(bool commit = true);
    }
}
