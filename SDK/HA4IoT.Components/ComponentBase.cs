using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;
using HA4IoT.Networking.Json;
using HA4IoT.Settings;

namespace HA4IoT.Components
{
    public abstract class ComponentBase : IComponent
    {
        private DateTime? _stateLastChanged;

        protected ComponentBase(ComponentId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;

            Settings = new SettingsContainer(StoragePath.WithFilename("Components", id.Value, "Settings.json"));
            GeneralSettingsWrapper = new ComponentSettingsWrapper(Settings);
        }

        public event EventHandler<ComponentStateChangedEventArgs> StateChanged;

        public ComponentId Id { get; }

        public ISettingsContainer Settings { get; }

        public IActuatorSettingsWrapper GeneralSettingsWrapper { get; }

        public abstract IComponentState GetState();

        public abstract IList<IComponentState> GetSupportedStates();

        public virtual JsonObject ExportConfigurationToJsonObject()
        {
            var configuration = new JsonObject();
            configuration.SetValue(ComponentConfigurationKey.Type, GetType().Name);
            configuration.SetValue(ComponentConfigurationKey.Settings, Settings.Export());

            var supportedStates = GetSupportedStates();
            if (supportedStates != null)
            {
                var supportedStatesJson = new JsonArray();
                foreach (var supportedState in supportedStates)
                {
                    supportedStatesJson.Add(supportedState.ToJsonValue());
                }

                configuration.SetValue(ComponentConfigurationKey.SupportedStates, supportedStatesJson);
            }
            
            return configuration;
        }

        public virtual JsonObject ExportStatusToJsonObject()
        {
            var status = new JsonObject();
            status.SetValue(ComponentConfigurationKey.Settings, Settings.Export());
            status.SetNamedValue(ActuatorStatusKey.State, GetState().ToJsonValue());
            status.SetValue(ActuatorStatusKey.StateLastChanged, _stateLastChanged);
            return status;
        }

        public virtual void HandleApiCall(IApiContext apiContext)
        {
        }

        protected void OnActiveStateChanged(IComponentState oldState, IComponentState newState)
        {
            _stateLastChanged = DateTime.Now;

            Log.Info($"Component '{Id}' updated state from '{oldState}' to '{newState}'");
            StateChanged?.Invoke(this, new ComponentStateChangedEventArgs(oldState, newState));
        }
    }
}