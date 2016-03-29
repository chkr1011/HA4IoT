using System;

namespace HA4IoT.Contracts.Sensors
{
    public interface ICasement
    {
        string Id { get; }

        event EventHandler<CasementStateChangedEventArgs> StateChanged;

        CasementState GetState();
    }
}