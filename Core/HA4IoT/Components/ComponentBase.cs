using System;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Components
{
    public abstract class ComponentBase : IComponent
    {
        protected ComponentBase(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public event EventHandler<ComponentFeatureStateChangedEventArgs> StateChanged;

        public string Id { get; }

        public abstract IComponentFeatureStateCollection GetState();

        public abstract IComponentFeatureCollection GetFeatures();

        public virtual void ExecuteCommand(ICommand command)
        {
            throw new CommandNotSupportedException(command);
        }

        protected void OnStateChanged(IComponentFeatureStateCollection oldState)
        {
            StateChanged?.Invoke(this, new ComponentFeatureStateChangedEventArgs(oldState, GetState()));;
        }

        protected void OnStateChanged(IComponentFeatureStateCollection oldState, IComponentFeatureStateCollection newState)
        {
            StateChanged?.Invoke(this, new ComponentFeatureStateChangedEventArgs(oldState, newState));
        }
    }
}