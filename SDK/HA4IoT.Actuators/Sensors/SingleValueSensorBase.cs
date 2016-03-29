using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class SingleValueSensorBase : ActuatorBase, ISingleValueSensor
    {
        private DateTime? _valueLastChanged;
        private float _value;

        protected SingleValueSensorBase(ActuatorId id)
            : base(id)
        {
        }

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        public float GetValue()
        {
            return _value;
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedValue("value", _value.ToJsonValue());
            status.SetNamedValue("valueLastChanged", _valueLastChanged.ToJsonValue());

            return status;
        }

        protected void SetValueInternal(float newValue)
        {
            float oldValue = _value;
            if (Math.Abs(oldValue - newValue) < Settings.GetFloat(SingleValueSensorSettings.MinDelta))
            {
                return;
            }

            _value = newValue;
            _valueLastChanged = DateTime.Now;

            Log.Info($"{Id}:{oldValue}->{newValue}");
            ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, _value));

            NotifyStateChanged();
        }
    }
}
