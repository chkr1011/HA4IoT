using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Tests.Mockups
{
    public class TestSensor : ISingleValueSensorActuator
    {
        private float _value;

        public TestSensor(ActuatorId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = Id;
        }

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;
        
        public ActuatorId Id { get; }

        public ISettingsContainer Settings { get; }
        public IActuatorSettingsWrapper GeneralSettingsWrapper { get; }

        public float GetValue()
        {
            return _value;
        }

        public void SetValue(float value)
        {
            var oldValue = _value;
            _value = value;

            ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, _value));
        }

        public JsonObject ExportConfigurationToJsonObject()
        {
            throw new NotImplementedException();
        }

        public JsonObject ExportStatusToJsonObject()
        {
            throw new NotImplementedException();
        }

        public void ExposeToApi(IApiController apiController)
        {
        }
    }
}