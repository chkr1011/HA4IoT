using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutter : IActuator
    {
        event EventHandler<RollerShutterStateChangedEventArgs> StateChanged;

        bool IsClosed { get; }

        RollerShutterState GetState();

        void SetState(RollerShutterState state);
    }
}
