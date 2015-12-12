using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutter : IActuator, IActuatorWithOffState
    {
        event EventHandler<RollerShutterStateChangedEventArgs> StateChanged;

        RollerShutterState GetState();

        void SetState(RollerShutterState state);

        bool IsClosed { get; }
    }
}
