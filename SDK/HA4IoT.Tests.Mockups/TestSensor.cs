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

        public event EventHandler<ActuatorIsEnabledChangedEventArgs> IsEnabledChanged;

        public float InternalValue
        {
            get { return _internalValue; }

            set
            {
                float oldValue = _internalValue;
                _internalValue = value;
                ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, _internalValue));
            }
        }


        public ActuatorId Id { get; }
        public bool IsEnabled { get; }

        public float GetValue()
        {
            return InternalValue;
        }

        public JsonObject GetConfigurationForApi()
        {
            throw new NotImplementedException();
        }

        public JsonObject GetStatusForApi()
        {
            throw new NotImplementedException();
        }
    }
}
