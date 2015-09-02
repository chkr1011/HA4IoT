using System;

namespace CK.HomeAutomation.Actuators
{
    public interface IBinaryStateOutputActuator
    {
        event EventHandler StateChanged; 

        string Id { get; }

        BinaryActuatorState State { get; }

        void SetState(BinaryActuatorState state, bool commit = true);

        void Toggle(bool commit);
    }
}
