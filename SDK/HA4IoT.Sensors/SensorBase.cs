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

namespace HA4IoT.Sensors
{
    public abstract class SensorBase : ISensor
    {
        private ISensorValue _currentValue = new NumericSensorValue(0);
        private DateTime? _valueLastChanged;

        protected SensorBase(ComponentId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;

            Settings = new SettingsContainer(StoragePath.WithFilename("Sensors", id.Value, "Settings.json"));
            GeneralSettingsWrapper = new ActuatorSettingsWrapper(Settings);
        }

        public event EventHandler<SensorValueChangedEventArgs> CurrentValueChanged;

        public ComponentId Id { get; }

        public ISettingsContainer Settings { get; }

        public IActuatorSettingsWrapper GeneralSettingsWrapper { get; }

        protected IApiController ApiController { get; set; }

        public virtual JsonObject ExportStatusToJsonObject()
        {
            var result = new JsonObject();
            result.SetNamedObject("settings", Settings.Export());
            result.SetNamedValue("value", GetCurrentValue().ToJsonValue());
            result.SetNamedValue("valueLastChanged", _valueLastChanged.ToJsonValue());

            return result;
        }
        
        public ISensorValue GetCurrentValue()
        {
            return _currentValue;
        }

        public virtual JsonObject ExportConfigurationToJsonObject()
        {
            var result = new JsonObject();
            result.SetNamedValue("type", GetType().FullName.ToJsonValue());
            result.SetNamedValue("settings", Settings.Export());

            return result;
        }

        public void ExposeToApi(IApiController apiController)
        {
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            new ActuatorSettingsApiDispatcher(this, apiController).ExposeToApi();

            apiController.RouteCommand($"sensor/{Id}/status", HandleApiCommand);
            apiController.RouteRequest($"sensor/{Id}/status", HandleApiRequest);
            
            ApiController = apiController;
        }

        protected void SetCurrentValue(ISensorValue newValue)
        {
            if (newValue == null) throw new ArgumentNullException(nameof(newValue));

            if (newValue.Equals(_currentValue))
            {
                return;
            }
           
            var oldValue = _currentValue;
            _currentValue = newValue;
            _valueLastChanged = DateTime.Now;
            OnCurrentValueChanged(oldValue, newValue);
        }

        protected virtual void HandleApiCommand(IApiContext apiContext)
        {
        }

        protected virtual void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response = ExportStatusToJsonObject();
        }

        protected void OnCurrentValueChanged(ISensorValue oldValue, ISensorValue newValue)
        {
            Log.Info($"Sensor '{Id}' updated value from '{oldValue}' to '{newValue}'");

            ApiController?.NotifyStateChanged(this);
            CurrentValueChanged?.Invoke(this, new SensorValueChangedEventArgs(oldValue, newValue));
        }
    }
}