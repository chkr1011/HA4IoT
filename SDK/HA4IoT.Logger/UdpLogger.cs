using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Windows.Data.Json;
using Windows.Networking;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Logger
{
    public class UdpLogger : ILogger
    {
        private readonly bool _isDebuggerAttached = Debugger.IsAttached;

        private readonly object _syncRoot = new object();
        private readonly List<LogEntry> _history = new List<LogEntry>(100);
        private readonly List<LogEntry> _items = new List<LogEntry>(1000);

        private readonly Timer _timer;
        
        private DatagramSocket _socket;
        private Stream _outputStream;

        private long _currentId;

        public UdpLogger()
        {
            _timer = new Timer(SendQueuedItems, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            _socket = new DatagramSocket();
            _socket.Control.DontFragment = true;
            _socket.ConnectAsync(new HostName("255.255.255.255"), "19227").AsTask().Wait();
            
            _outputStream = _socket.OutputStream.AsStreamForWrite();

            _timer.Change(0, Timeout.Infinite);
        }

        public void ExposeToApi(IApiService apiService)
        {
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            apiService.RouteRequest("trace", HandleApiGet);
        }

        public void Verbose(string message)
        {
            Publish(LogEntrySeverity.Verbose, message);
        }

        public void Info(string message)
        {
            Publish(LogEntrySeverity.Info, message);
        }

        public void Warning(string message)
        {
            Publish(LogEntrySeverity.Warning, message);
        }

        public void Warning(Exception exception, string message)
        {
            Publish(LogEntrySeverity.Warning, message + Environment.NewLine + exception);
        }

        public void Error(string message)
        {
            Publish(LogEntrySeverity.Error, message);
        }

        public void Error(Exception exception, string message)
        {
            Publish(LogEntrySeverity.Error, message + Environment.NewLine + exception);
        }

        private void Publish(LogEntrySeverity type, string message, params object[] parameters)
        {
            if (parameters != null && parameters.Any())
            {
                try
                {
                    message = string.Format(message, parameters);
                }
                catch (FormatException)
                {
                    message = message + " (" + string.Join(",", parameters) + ")";
                }
            }

            PrintNotification(type, message);

            // TODO: Refactor to use IHomeAutomationTimer.CurrentDateTime;
            var logEntry = new LogEntry(_currentId, DateTime.Now, Environment.CurrentManagedThreadId, type, string.Empty, message);
            lock (_syncRoot)
            {
                _items.Add(logEntry);
                _currentId++;

                UpdateHistory(logEntry);
            }
        }

        private void UpdateHistory(LogEntry logEntry)
        {
            if (logEntry.Severity != LogEntrySeverity.Verbose)
            {
                _history.Add(logEntry);

                if (_history.Count > 100)
                {
                    _history.RemoveAt(0);
                }
            }
        }

        private void HandleApiGet(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                apiContext.Response = CreatePackage(_history);
            }
        }

        private void SendQueuedItems(object state)
        {
            try
            {
                List<LogEntry> pendingItems = GetPendingItems();
                if (!pendingItems.Any())
                {
                    return;
                }

                foreach (var traceItem in pendingItems)
                {
                    var collection = new[] { traceItem };
                    JsonObject package = CreatePackage(collection);

                    string data = package.Stringify();
                    byte[] buffer = Encoding.UTF8.GetBytes(data);

                    _outputStream.Write(buffer, 0, buffer.Length);
                    _outputStream.Flush();
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("ERROR: Could not send trace items. " + exception);
            }
            finally
            {
                _timer.Change(50, Timeout.Infinite);
            }
        }

        private List<LogEntry> GetPendingItems()
        {
            lock (_syncRoot)
            {
                var pendingItems = new List<LogEntry>(_items);
                _items.Clear();

                return pendingItems;
            }
        }

        private JsonObject CreatePackage(IEnumerable<LogEntry> traceItems)
        {
            var traceItemsCollection = new JsonArray();
            foreach (var traceItem in traceItems)
            {
                traceItemsCollection.Add(traceItem.ExportToJsonObject());
            }
            
            JsonObject package = new JsonObject();
            package.SetNamedValue("Type", "HA4IoT.Trace".ToJsonValue());
            package.SetNamedValue("Version", 1.ToJsonValue());
            package.SetNamedValue("TraceItems", traceItemsCollection);

            return package;
        }

        private void PrintNotification(LogEntrySeverity type, string message)
        {
            if (!_isDebuggerAttached)
            {
                return;
            }

            string typeText = string.Empty;
            switch (type)
            {
                case LogEntrySeverity.Error:
                    {
                        typeText = "ERROR";
                        break;
                    }

                case LogEntrySeverity.Info:
                    {
                        typeText = "INFO";
                        break;
                    }

                case LogEntrySeverity.Warning:
                    {
                        typeText = "WARNING";
                        break;
                    }

                case LogEntrySeverity.Verbose:
                    {
                        typeText = "VERBOSE";
                        break;
                    }
            }

            Debug.WriteLine(typeText + ": " + message);
        }
    }
}
