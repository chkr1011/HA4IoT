using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Notifications;

namespace HA4IoT.Notifications
{
    internal sealed class NotificationItem
    {
        public NotificationItem(DateTime timestamp, NotificationType type, string message)
        {
            Timestamp = timestamp;
            Type = type;
            Message = message;
        }

        public DateTime Timestamp { get; }

        public NotificationType Type { get; }

        public string Message { get; }

        public JsonObject ToJsonObject()
        {
            var notification = new JsonObject();
            notification.SetNamedValue("timestamp", JsonValue.CreateStringValue(Timestamp.ToString("O")));
            notification.SetNamedValue("type", JsonValue.CreateStringValue(Type.ToString()));
            notification.SetNamedValue("message", JsonValue.CreateStringValue(Message));

            return notification;
        }
    }
}
