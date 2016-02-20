using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups
{
    public class TestSensor : ISingleValueSensorActuator
    {
        private float _internalValue;

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        public ActuatorId Id { get; }
        public IActuatorSettings Settings { get; }

        public float InternalValue
        {
            get { return _internalValue; }

            set
            {
                var oldValue = _internalValue;
                _internalValue = value;
                ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, _internalValue));
            }
        }

        public float GetValue()
        {
            return InternalValue;
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
    }
}