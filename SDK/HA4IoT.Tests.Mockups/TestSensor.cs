using System;
using Windows.Data.Json;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups
{
    public class TestSensor : ISingleValueSensorActuator
    {
        private float _value;

        public TestSensor(ActuatorId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = Id;
            Settings = new ActuatorSettings(id);
        }

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;
        
        public ActuatorId Id { get; }

        public IActuatorSettings Settings { get; }

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

        public void LoadSettings()
        {
        }

        public void ExposeToApi()
        {
        }
    }
}