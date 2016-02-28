using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class SingleValueSensorActuatorBase<TSettings> : ActuatorBase<TSettings>, ISingleValueSensor where TSettings : ActuatorSettings
    {
        private DateTime? _valueLastChanged;
        private float _value;

        protected SingleValueSensorActuatorBase(ActuatorId id, IHttpRequestController httpApiController, ILogger logger)
            : base(id, httpApiController, logger)
        {
        }

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        // TODO: Move to dedicated Actuator Settings (TemperatureSensorSettings / HumiditySensorSettings).
        public float ValueChangedMinDelta { get; set; } = 0.15F;

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
            if (Math.Abs(oldValue - newValue) < ValueChangedMinDelta)
            {
                return;
            }

            _value = newValue;
            _valueLastChanged = DateTime.Now;

            Logger.Info($"{Id}:{oldValue}->{newValue}");
            ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, _value));
        }
    }
}
