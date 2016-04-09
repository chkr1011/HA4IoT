using System;
using Windows.Data.Json;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase : IActuator
    {
        private DateTime? _stateLastChanged;

        protected ActuatorBase(ComponentId id)
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

        public virtual JsonObject ExportStatusToJsonObject()
        {
            var result = new JsonObject();
            result.SetNamedObject("settings", Settings.Export());
            result.SetNamedValue("state", GetState().ToJsonValue());

            return result;
        }

        public abstract IComponentState GetState();

        public abstract void SetState(IComponentState state, params IHardwareParameter[] parameters);
        
        public virtual JsonObject ExportConfigurationToJsonObject()
        {
            var result = new JsonObject();
            result.SetNamedString("type", GetType().Name);
            result.SetNamedObject("settings", Settings.Export());
            result.SetNamedValue("state", GetState().ToJsonValue());
            result.SetNamedDateTime("stateLastChanged", _stateLastChanged);

            return result;
        }

        public virtual void HandleApiCommand(IApiContext apiContext)
        {
        }

        public virtual void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response = ExportStatusToJsonObject();
        }

        protected void OnActiveStateChanged(IComponentState oldState, IComponentState newState)
        {
            _stateLastChanged = DateTime.Now;

            Log.Info($"Actuator '{Id}' updated state from '{oldState}' to '{newState}'");
            StateChanged?.Invoke(this, new ComponentStateChangedEventArgs(oldState, newState));
        }
    }
}