using System;
using HA4IoT.Contracts.Actions;

namespace HA4IoT.Contracts.Triggers
{
    public interface ITrigger
    {
        event EventHandler<TriggeredEventArgs> Triggered;

        bool IsAnyAttached { get; }

        void Attach(Action action);

        void Attach(IAction action);
    }
}
