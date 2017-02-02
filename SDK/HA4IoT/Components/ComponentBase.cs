using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Components
{
    public abstract class ComponentBase : IComponent
    {
        protected ComponentBase(ComponentId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public event EventHandler<ComponentFeatureStateChangedEventArgs> StateChanged;

        public ComponentId Id { get; }

        public abstract ComponentFeatureStateCollection GetState();

        public abstract ComponentFeatureCollection GetFeatures();

        public abstract void InvokeCommand(ICommand command);

        protected void OnStateChanged<TComponentFeatureState>(TComponentFeatureState oldState, TComponentFeatureState newState) where TComponentFeatureState : IComponentFeatureState
        {
            var oldStateText = oldState?.Serialize();
            var newStateText = newState?.Serialize();

            Log.Info($"Component '{Id}' updated state '{typeof(TComponentFeatureState).Name}' from '{oldStateText}' to '{newStateText}'");
            StateChanged?.Invoke(this, new ComponentFeatureStateChangedEventArgs(oldState, newState));
        }











        #region OLD

        public virtual IList<GenericComponentState> GetSupportedStates()
        {
            return new List<GenericComponentState>();
        }

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
                ["State"] = JObject.FromObject(GetState().Serialize())
            };

            return status;
        }
        #endregion


    }
}