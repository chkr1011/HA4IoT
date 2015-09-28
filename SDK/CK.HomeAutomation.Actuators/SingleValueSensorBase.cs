using System;
using Windows.Data.Json;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public abstract class SingleValueSensorBase : ActuatorBase
    {
        protected SingleValueSensorBase(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
        }

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        public float Value { get; private set; }

        public override void ApiGet(ApiRequestContext context)
        {
            context.Response.SetNamedValue("value", JsonValue.CreateNumberValue(Value));
        }

        protected void UpdateValue(float newValue)
        {
            float oldValue = Value;
            if (Math.Abs(oldValue - newValue) > 0.15)
            {
                Value = newValue;

                NotificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' updated the value to from '{1}' to '{2}'", Id, oldValue, Value);
                ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, Value));
            }
        }
    }
}
