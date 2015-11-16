using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace HA4IoT.Notifications
{
    public class NotificationHandler : INotificationHandler
    {
        private readonly bool _isDebuggerAttached = Debugger.IsAttached;
        private readonly List<NotificationItem> _items = new List<NotificationItem>();
        private readonly object _syncRoot = new object();

        public NotificationHandler()
        {
            Task.Factory.StartNew(SendAsync, TaskCreationOptions.LongRunning);
        }

        public void Publish(NotificationType type, string text, params object[] parameters)
        {
            if (parameters != null && parameters.Any())
            {
                try
                {
                    text = string.Format(text, parameters);
                }
                catch (FormatException)
                {
                    text = text + " (" + string.Join(",", parameters) + ")";
                }
            }

            PrintNotification(type, text);

            lock (_syncRoot)
            {
                _items.Add(new NotificationItem(type, text));
            }
        }

        public void PublishFrom<TSender>(TSender sender, NotificationType type, string message, params object[] parameters) where TSender : class
        {
            string senderText = typeof(TSender).Name;
            Publish(type, senderText + ": " + message, parameters);
        }

        private async void SendAsync()
        {
            using (DatagramSocket socket = new DatagramSocket())
            {
                socket.Control.DontFragment = true;

                using (IOutputStream streamReader = socket.GetOutputStreamAsync(new HostName("255.255.255.255"), "19227").AsTask().Result)
                using (var writer = new DataWriter(streamReader))
                {
                    while (true)
                    {
                        try
                        {
                            var pendingItems = GetPendingItems();
                            if (pendingItems.Any())
                            {
                                var package = CreatePackage(pendingItems);

                                await writer.FlushAsync();
                                writer.WriteString(package.Stringify());
                                await writer.StoreAsync();
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine("Could not send notifications. " + exception.Message);
                        }

                        await Task.Delay(100);
                    }
                }
            }
        }

        private List<NotificationItem> GetPendingItems()
        {
            List<NotificationItem> itemsToSend;
            lock (_syncRoot)
            {
                itemsToSend = new List<NotificationItem>(_items);
                _items.Clear();
            }

            return itemsToSend;
        }

        private JsonObject CreatePackage(ICollection<NotificationItem> notificationItems)
        {
            JsonArray notifications = new JsonArray();
            foreach (var notificationItem in notificationItems)
            {
                var notification = new JsonObject();
                notification.SetNamedValue("type", JsonValue.CreateStringValue(notificationItem.Type.ToString()));
                notification.SetNamedValue("message", JsonValue.CreateStringValue(notificationItem.Message));

                notifications.Add(notification);
            }

            JsonObject package = new JsonObject();
            package.SetNamedValue("type", JsonValue.CreateStringValue("HA4IoT.Notifications"));
            package.SetNamedValue("version", JsonValue.CreateNumberValue(1));
            package.SetNamedValue("notifications", notifications);

            return package;
        }

        private void PrintNotification(NotificationType type, string message)
        {
            if (!_isDebuggerAttached)
            {
                return;
            }

            string typeText = string.Empty;
            switch (type)
            {
                case NotificationType.Error:
                    {
                        typeText = "ERROR";
                        break;
                    }

                case NotificationType.Info:
                    {
                        typeText = "INFO";
                        break;
                    }

                case NotificationType.Warning:
                    {
                        typeText = "WARNING";
                        break;
                    }

                case NotificationType.Verbose:
                    {
                        typeText = "VERBOSE";
                        break;
                    }
            }

            Debug.WriteLine(typeText + ": " + message);
        }
    }
}
