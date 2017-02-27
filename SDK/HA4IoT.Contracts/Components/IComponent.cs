using System;
using HA4IoT.Contracts.Commands;

namespace HA4IoT.Contracts.Components
{
    public interface IComponent
    {
        event EventHandler<ComponentFeatureStateChangedEventArgs> StateChanged;

        string Id { get; }

        ComponentFeatureStateCollection GetState();

        ComponentFeatureCollection GetFeatures();

        void ExecuteCommand(ICommand command);
    }
}
