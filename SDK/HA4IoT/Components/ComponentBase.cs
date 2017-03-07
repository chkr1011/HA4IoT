using System;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Components
{
    public abstract class ComponentBase : IComponent
    {
        protected ComponentBase(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public event EventHandler<ComponentFeatureStateChangedEventArgs> StateChanged;

        public string Id { get; }

        protected ILogger Log { get; }

        public abstract ComponentFeatureStateCollection GetState();

        public abstract ComponentFeatureCollection GetFeatures();

        public virtual void ExecuteCommand(ICommand command)
        {
            throw new CommandNotSupportedException(command);
        }

        protected void OnStateChanged(ComponentFeatureStateCollection oldState)
        {
            OnStateChanged(oldState, GetState());
        }

        protected void OnStateChanged(ComponentFeatureStateCollection oldState, ComponentFeatureStateCollection newState)
        {
            StateChanged?.Invoke(this, new ComponentFeatureStateChangedEventArgs(oldState, newState));
        }
    }
}