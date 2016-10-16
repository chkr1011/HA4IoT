using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json.Linq;

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

        public abstract ComponentState GetState();

        public abstract IList<ComponentState> GetSupportedStates();

        public virtual void HandleApiCall(IApiContext apiContext)
        {
        }

        public virtual JToken ExportConfiguration()
        {
            var configuration = new JObject
            {
                ["Type"] = GetType().Name
            };

            return configuration;
        }

        public virtual JToken ExportStatus()
        {
            var status = new JObject
            {
                ["State"] = GetState().JToken,
                ["StateLastChanged"] = _stateLastChanged
            };

            return status;
        }

        protected void OnActiveStateChanged(ComponentState oldState, ComponentState newState)
        {
            _stateLastChanged = DateTime.Now;

            Log.Info($"Component '{Id}' updated state from '{oldState}' to '{newState}'");
            StateChanged?.Invoke(this, new ComponentStateChangedEventArgs(oldState, newState));
        }
    }
}