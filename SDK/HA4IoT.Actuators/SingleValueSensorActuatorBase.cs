using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class SingleValueSensorActuatorBase : ActuatorBase, ISingleValueSensor
    {
        private float _value;

        protected SingleValueSensorActuatorBase(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
        }

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        public float ValueChangedMinDelta { get; set; } = 0.15F;

        public float GetValue()
        {
            return _value;
        }

        public override void HandleApiGet(ApiRequestContext context)
        {
            context.Response.SetNamedValue("value", JsonValue.CreateNumberValue(_value));
        }

        public void UpdateValue(float newValue)
        {
            float oldValue = _value;
            if (Math.Abs(oldValue - newValue) > ValueChangedMinDelta)
            {
                _value = newValue;

                NotificationHandler.Info(Id + ": " + oldValue + "->" + newValue);
                ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, _value));
            }
        }
    }
}
