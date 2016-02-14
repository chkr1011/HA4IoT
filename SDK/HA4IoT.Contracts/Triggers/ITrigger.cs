using System;

namespace HA4IoT.Contracts.Triggers
{
    public interface ITrigger
    {
        event EventHandler<TriggeredEventArgs> Triggered;

        bool IsAnyAttached { get; }

        void Attach(Action action);
    }
}
