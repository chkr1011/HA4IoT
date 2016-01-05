using System;
using Windows.Data.Json;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class SingleValueSensorActuatorBase : ActuatorBase, ISingleValueSensor
    {
        private float _value;

        protected SingleValueSensorActuatorBase(ActuatorId id, IHttpRequestController api, INotificationHandler logger)
            : base(id, api, logger)
        {
        }

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        public float ValueChangedMinDelta { get; set; } = 0.15F;

        public float GetValue()
        {
            return _value;
        }

        public override JsonObject GetStatusForApi()
        {
            var status = base.GetStatusForApi();
            status.SetNamedValue("value", JsonValue.CreateNumberValue(_value));

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

            Logger.Info(Id + ": " + oldValue + "->" + newValue);
            ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, _value));
        }
    }
}
