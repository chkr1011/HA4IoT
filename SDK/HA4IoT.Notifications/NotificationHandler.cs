using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Notifications
{
    public class NotificationHandler : INotificationHandler
    {
        private readonly bool _isDebuggerAttached = Debugger.IsAttached;

        private readonly object _syncRoot = new object();
        private readonly List<NotificationItem> _items = new List<NotificationItem>();
        private readonly List<NotificationItem> _history = new List<NotificationItem>();
        
        public NotificationHandler()
        {
            Task.Factory.StartNew(SendAsync, TaskCreationOptions.LongRunning);
        }

        public void ExposeToApi(IHttpRequestController apiRequestController)
        {
            if (apiRequestController == null) throw new ArgumentNullException(nameof(apiRequestController));

            apiRequestController.Handle(HttpMethod.Get, "notifications").Using(HandleApiGet);
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
                var notification = new NotificationItem(DateTime.Now, type, text);

                _items.Add(notification);

                if (notification.Type != NotificationType.Verbose)
                {
                    _history.Add(notification);
                    if (_history.Count > 50)
                    {
                        _history.RemoveAt(0);
                    }
                }
            }
        }

        public void Info(string message, params object[] parameters)
        {
            Publish(NotificationType.Info, message, parameters);
        }

        public void Warning(string message, params object[] parameters)
        {
            Publish(NotificationType.Warning, message, parameters);
        }

        public void Error(string message, params object[] parameters)
        {
            Publish(NotificationType.Error, message, parameters);
        }

        public void Verbose(string message, params object[] parameters)
        {
            Publish(NotificationType.Verbose, message, parameters);
        }

        private void HandleApiGet(HttpContext httpContext)
        {
            lock (_syncRoot)
            {
                httpContext.Response.Body = new JsonBody(CreatePackage(_history));
            }
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
            foreach (var notification in notificationItems)
            {
                notifications.Add(notification.ToJsonObject());
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
