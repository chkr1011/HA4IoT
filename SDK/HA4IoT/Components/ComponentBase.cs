using System;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public abstract ComponentFeatureStateCollection GetState();

        public abstract ComponentFeatureCollection GetFeatures();

        public virtual void InvokeCommand(ICommand command)
        {
            throw new CommandNotSupportedException(command);
        }

        protected void OnStateChanged(ComponentFeatureStateCollection oldState)
        {
            OnStateChanged(oldState, GetState());
        }

        protected void OnStateChanged(ComponentFeatureStateCollection oldState, ComponentFeatureStateCollection newState)
        {
            var oldStateText = JToken.FromObject(oldState?.Serialize()).ToString(Formatting.None);
            var newStateText = JToken.FromObject(newState?.Serialize()).ToString(Formatting.None);

            Log.Info($"Component '{Id}' updated state from:{oldStateText} to:{newStateText}");
            StateChanged?.Invoke(this, new ComponentFeatureStateChangedEventArgs(oldState, newState));
        }
    }
}