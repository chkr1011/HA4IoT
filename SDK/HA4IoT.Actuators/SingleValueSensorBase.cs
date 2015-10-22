using System;
using Windows.Data.Json;
using HA4IoT.Hardware;
using HA4IoT.Networking;
using HA4IoT.Notifications;

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

                NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' updated the value to from '{1}' to '{2}'", Id, oldValue, Value);
                ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, Value));
            }
        }
    }
}
