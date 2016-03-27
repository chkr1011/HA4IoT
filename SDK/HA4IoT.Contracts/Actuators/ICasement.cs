using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface ICasement
    {
        string Id { get; }

        event EventHandler<CasementStateChangedEventArgs> StateChanged;

        CasementState GetState();
    }
}