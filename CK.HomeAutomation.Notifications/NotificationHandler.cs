using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace CK.HomeAutomation.Notifications
{
    public class NotificationHandler : INotificationHandler
    {
        private readonly bool _isDebuggerAttached = Debugger.IsAttached;
        private readonly List<NotificationItem> _items = new List<NotificationItem>();

        private readonly DatagramSocket _socket;

        private readonly object _syncRoot = new object();
        private readonly DataWriter _writer;

        public NotificationHandler()
        {
            _socket = new DatagramSocket();
            _socket.Control.DontFragment = true;

            IOutputStream streamReader = _socket.GetOutputStreamAsync(new HostName("255.255.255.255"), "19227").AsTask().Result;
            _writer = new DataWriter(streamReader);

            SendAsync();
        }

        public void Publish(NotificationType type, string text, params object[] parameters)
        {
            text = string.Format(text, parameters);
            PrintNotification(type, text);

            lock (_syncRoot)
            {
                _items.Add(new NotificationItem(type, text));
            }
        }

        public void PublishFrom<TSender>(TSender sender, NotificationType type, string message, params object[] parameters) where TSender : class
        {
            if (sender == null)
            {
                Publish(type, message, parameters);
            }
            else
            {
                string senderText = sender.GetType().Name;
                Publish(type, senderText + ": " + message, parameters);
            }
        }

        private async void SendAsync()
        {
            while (true)
            {
                List<NotificationItem> itemsToSend;
                lock (_syncRoot)
                {
                    itemsToSend = new List<NotificationItem>(_items);
                    _items.Clear();
                }

                foreach (var notificationItem in itemsToSend)
                {
                    try
                    {
                        string package = "CK.HA/1.0 " + ((int) notificationItem.Type) + " " + notificationItem.Message;

                        await _writer.FlushAsync();
                        _writer.WriteString(package);
                        await _writer.StoreAsync();
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine("Could not send HC notification '" + notificationItem.Type + ":" + notificationItem.Message + "'. " + exception.Message);
                    }
                }

                await Task.Delay(100);
            }
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
