using System;
using Windows.Data.Json;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;
using ComponentStateChangedEventArgs = HA4IoT.Contracts.Components.ComponentStateChangedEventArgs;

namespace HA4IoT.Sensors
{
    public abstract class SensorBase : ISensor
    {
        private IComponentState _state = new UnknownComponentState();
        private DateTime? _stateLastChanged;

        protected SensorBase(ComponentId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;

            Settings = new SettingsContainer(StoragePath.WithFilename("Components", id.Value, "Settings.json"));
            GeneralSettingsWrapper = new ActuatorSettingsWrapper(Settings);
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
            result.SetNamedValue("stateLastChanged", _stateLastChanged.ToJsonValue());

            return result;
        }

        public IComponentState GetState()
        {
            return _state;
        }

        public virtual JsonObject ExportConfigurationToJsonObject()
        {
            var result = new JsonObject();
            result.SetNamedString("type", GetType().Name);
            result.SetNamedValue("settings", Settings.Export());

            return result;
        }

        protected void SetState(IComponentState newState)
        {
            if (newState.Equals(_state))
            {
                return;
            }
           
            var oldValue = _state;
            _state = newState;

            OnCurrentValueChanged(oldValue, newState);
        }

        public virtual void HandleApiCommand(IApiContext apiContext)
        {
        }

        public virtual void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response = ExportStatusToJsonObject();
        }

        protected void OnCurrentValueChanged(IComponentState oldState, IComponentState newState)
        {
            Log.Info($"Sensor '{Id}' updated value from '{oldState}' to '{newState}'");

            _stateLastChanged = DateTime.Now;

            StateChanged?.Invoke(this, new ComponentStateChangedEventArgs(oldState, newState));
        }
    }
}