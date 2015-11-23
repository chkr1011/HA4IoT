using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class SingleValueSensorBase : ActuatorBase
    {
        protected SingleValueSensorBase(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
        }

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        public float Value { get; private set; }

        public float ValueChangedMinDelta { get; set; } = 0.15F;

        public override void ApiGet(ApiRequestContext context)
        {
            context.Response.SetNamedValue("value", JsonValue.CreateNumberValue(Value));
        }

        protected void UpdateValue(float newValue)
        {
            float oldValue = Value;
            if (Math.Abs(oldValue - newValue) > ValueChangedMinDelta)
            {
                Value = newValue;

                NotificationHandler.Info(Id + ": " + oldValue + "->" + newValue);
                ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, Value));
            }
        }
    }
}
