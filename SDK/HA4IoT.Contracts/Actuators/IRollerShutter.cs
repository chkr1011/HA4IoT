using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutter : IActuator, IActuatorWithOffState
    {
        event EventHandler<RollerShutterStateChangedEventArgs> StateChanged;

        bool IsClosed { get; }

        IRollerShutterSettings Settings { get; }

        RollerShutterState GetState();

        void SetState(RollerShutterState state);
    }
}
