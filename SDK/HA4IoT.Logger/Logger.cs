using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Logger
{
    public class Logger : ILogger
    {
        private readonly bool _isDebuggerAttached = Debugger.IsAttached;

        private readonly object _syncRoot = new object();
        private readonly List<LogEntry> _history = new List<LogEntry>();

        private List<LogEntry> _items = new List<LogEntry>();
        private List<LogEntry> _itemsBuffer = new List<LogEntry>();

        private long _currentId;

        public Logger()
        {
            Task.Factory.StartNew(SendQueuedItems, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void ExposeToApi(IHttpRequestController httpApiController)
        {
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            httpApiController.HandleGet("trace").Using(HandleApiGet);
        }

        public void Info(string message, params object[] parameters)
        {
            Publish(LogEntrySeverity.Info, message, parameters);
        }

        public void Warning(string message, params object[] parameters)
        {
            Publish(LogEntrySeverity.Warning, message, parameters);
        }

        public void Warning(Exception exception, string message, params object[] parameters)
        {
            Publish(LogEntrySeverity.Warning, string.Format(message, parameters) + Environment.NewLine + exception);
        }

        public void Error(string message, params object[] parameters)
        {
            Publish(LogEntrySeverity.Error, message, parameters);
        }

        public void Error(Exception exception, string message, params object[] parameters)
        {
            Publish(LogEntrySeverity.Error, string.Format(message, parameters) + Environment.NewLine + exception);
        }

        public void Verbose(string message, params object[] parameters)
        {
            Publish(LogEntrySeverity.Verbose, message, parameters);
        }

        private void Publish(LogEntrySeverity type, string text, params object[] parameters)
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

            var logEntry = new LogEntry(_currentId, DateTime.Now, Environment.CurrentManagedThreadId, type, text);
            lock (_syncRoot)
            {
                _items.Add(logEntry);
                _currentId++;

                if (logEntry.Severity != LogEntrySeverity.Verbose)
                {
                    _history.Add(logEntry);

                    if (_history.Count > 100)
                    {
                        _history.RemoveAt(0);
                    }
                }
            }
        }

        private void HandleApiGet(HttpContext httpContext)
        {
            lock (_syncRoot)
            {
                httpContext.Response.Body = new JsonBody(CreatePackage(_history));
            }
        }

        private async Task SendQueuedItems()
        {
            using (DatagramSocket socket = new DatagramSocket())
            {
                await socket.ConnectAsync(new HostName("255.255.255.255"), "19227");

                Stream outputStream = socket.OutputStream.AsStreamForWrite();
                while (true)
                {
                    List<LogEntry> pendingItems = GetPendingItems();
                    try
                    {
                        foreach (var traceItem in pendingItems)
                        {
                            var collection = new[] {traceItem};
                            JsonObject package = CreatePackage(collection);

                            string data = package.Stringify();
                            byte[] buffer = Encoding.UTF8.GetBytes(data);

                            outputStream.Write(buffer, 0, buffer.Length);
                            outputStream.Flush();
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine("ERROR: Could not send trace items. " + exception);
                    }
                    finally
                    {
                        pendingItems.Clear();
                    }

                    await Task.Delay(50);
                }
            }
        }

        private List<LogEntry> GetPendingItems()
        {
            lock (_syncRoot)
            {
                var buffer = _items;
                _items = _itemsBuffer;
                _itemsBuffer = buffer;

                return _itemsBuffer;
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
