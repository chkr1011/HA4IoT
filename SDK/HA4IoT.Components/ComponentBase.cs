using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking.Json;

namespace HA4IoT.Components
{
    public abstract class ComponentBase : IComponent
    {
        private DateTime? _stateLastChanged;

        protected ComponentBase(ComponentId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public event EventHandler<ComponentStateChangedEventArgs> StateChanged;

        public ComponentId Id { get; }
        
        public abstract IComponentState GetState();

        public abstract IList<IComponentState> GetSupportedStates();

        public virtual void HandleApiCall(IApiContext apiContext)
        {
        }

        public virtual JsonObject ExportConfiguration()
        {
            var configuration = new JsonObject();
            configuration.SetValue("Type", GetType().Name);

            return configuration;
        }

        public virtual JsonObject ExportStatus()
        {
            var status = new JsonObject();
            status.SetNamedValue(ActuatorStatusKey.State, GetState().ToJsonValue());
            status.SetValue(ActuatorStatusKey.StateLastChanged, _stateLastChanged);
            return status;
        }

        protected void OnActiveStateChanged(IComponentState oldState, IComponentState newState)
        {
            _stateLastChanged = DateTime.Now;

            Log.Info($"Component '{Id}' updated state from '{oldState}' to '{newState}'");
            StateChanged?.Invoke(this, new ComponentStateChangedEventArgs(oldState, newState));
        }
    }
}