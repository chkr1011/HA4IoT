using System;
using Windows.Data.Json;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public abstract class BaseSensor : BaseActuator
    {
        protected BaseSensor(string id, HttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
        }

        public event EventHandler ValueChanged;

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
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
