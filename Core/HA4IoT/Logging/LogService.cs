using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Logging
{
    [ApiServiceClass(typeof(ILogService))]
    public class LogService : ServiceBase, ILogService
    {
        private readonly object _syncRoot = new object();
        private readonly Guid _sessionId = Guid.NewGuid();

        private readonly List<LogEntry> _pendingLogEntries = new List<LogEntry>();
        private readonly RollingCollection<LogEntry> _logEntries = new RollingCollection<LogEntry>(1000);
        private readonly RollingCollection<LogEntry> _errorLogEntries = new RollingCollection<LogEntry>(250);
        private readonly RollingCollection<LogEntry> _warningLogEntries = new RollingCollection<LogEntry>(250);

        private readonly IEnumerable<ILogAdapter> _adapters;

        private long _id;

        public LogService(IEnumerable<ILogAdapter> adapters)
        {
            _adapters = adapters ?? throw new ArgumentNullException(nameof(adapters));

            Log.Default = CreatePublisher(null);
            Task.Factory.StartNew(ProcessPendingLogEntries, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public int ErrorsCount => _errorLogEntries.Count;
        public int WarningsCount => _warningLogEntries.Count;

        public ILogger CreatePublisher(string source)
        {
            return new LogServicePublisher(source, this);
        }

        public void Verbose(string message)
        {
            Verbose(null, message);
        }

        public void Verbose(string source, string message)
        {
            Publish(LogEntrySeverity.Verbose, source, message, null);
        }

        public void Info(string message)
        {
            Info(null, message);
        }

        public void Info(string source, string message)
        {
            Publish(LogEntrySeverity.Info, source, message, null);
        }

        public void Warning(string message)
        {
            Warning((string)null, message);
        }

        public void Warning(string source, string message)
        {
            Warning(source, null, message);
        }

        public void Warning(Exception exception, string message)
        {
            Warning(null, exception, message);
        }

        public void Warning(string source, Exception exception, string message)
        {
            Publish(LogEntrySeverity.Warning, source, message, exception);
        }

        public void Error(string message)
        {
            Error((string)null, message);
        }

        public void Error(string source, string message)
        {
            Error(source, null, message);
        }

        public void Error(Exception exception, string message)
        {
            Error(null, exception, message);
        }

        public void Error(string source, Exception exception, string message)
        {
            Publish(LogEntrySeverity.Error, source, message, exception);
        }

        public void Publish(LogEntrySeverity severity, string source, string message, Exception exception)
        {
            var logEntry = new LogEntry(DateTime.Now, global::System.Environment.CurrentManagedThreadId, severity, source ?? "System", message, exception?.ToString());
            lock (_pendingLogEntries)
            {
                _pendingLogEntries.Add(logEntry);
            }
        }

        [ApiMethod]
        public void ResetWarningLogEntries(IApiCall apiCall)
        {
            lock (_syncRoot)
            {
                _warningLogEntries.Clear();
            }
        }

        [ApiMethod]
        public void GetWarningLogEntries(IApiCall apiCall)
        {
            lock (_syncRoot)
            {
                CreateGetLogEntriesResponse(_warningLogEntries, apiCall);
            }
        }

        [ApiMethod]
        public void ResetErrorLogEntries(IApiCall apiCall)
        {
            lock (_syncRoot)
            {
                _errorLogEntries.Clear();
            }
        }

        [ApiMethod]
        public void GetErrorLogEntries(IApiCall apiCall)
        {
            lock (_syncRoot)
            {
                CreateGetLogEntriesResponse(_errorLogEntries, apiCall);
            }
        }

        [ApiMethod]
        public void GetLogEntries(IApiCall apiCall)
        {
            var request = apiCall.Parameter.ToObject<GetLogEntriesRequest>();
            if (request == null)
            {
                apiCall.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            if (request.MaxCount == 0)
            {
                request.MaxCount = 100;
            }

            List<LogEntry> logEntries;
            lock (_syncRoot)
            {
                if (!request.SessionId.HasValue || request.SessionId.Value != _sessionId)
                {
                    logEntries = _logEntries.Take(request.MaxCount).ToList();
                }
                else
                {
                    logEntries = _logEntries.SkipWhile(e => e.Id <= request.Offset).Take(request.MaxCount).ToList();
                }
            }

            var response = new GetLogEntriesResponse
            {
                SessionId = _sessionId,
                LogEntries = logEntries
            };

            apiCall.Result = JObject.FromObject(response);
        }

        private void PublishInternal(LogEntry logEntry)
        {
            lock (_syncRoot)
            {
                logEntry.Id = Interlocked.Increment(ref _id);
                _logEntries.Add(logEntry);

                if (logEntry.Severity == LogEntrySeverity.Warning)
                {
                    _warningLogEntries.Add(logEntry);
                }
                else if (logEntry.Severity == LogEntrySeverity.Error)
                {
                    _errorLogEntries.Add(logEntry);
                }
            }

            foreach (var adapter in _adapters)
            {
                try
                {
                    adapter.ProcessLogEntry(logEntry);
                }
                catch
                {
                    // Do nothing here. Otherwise the app will crash when trying to log a broken log component.
                }
            }

            if (Debugger.IsAttached)
            {
                Debug.WriteLine($"[{logEntry.Severity}] [{logEntry.Source}] [{logEntry.ThreadId}]: {logEntry.Message}");
            }
        }

        private void CreateGetLogEntriesResponse(IEnumerable<LogEntry> source, IApiCall apiCall)
        {
            var response = new GetLogEntriesResponse
            {
                SessionId = _sessionId,
                LogEntries = source.ToList()
            };

            apiCall.Result = JObject.FromObject(response);
        }

        private void ProcessPendingLogEntries()
        {
            var buffer = new List<LogEntry>();
            while (true)
            {
                Task.Delay(100).Wait();

                lock (_pendingLogEntries)
                {
                    buffer.AddRange(_pendingLogEntries);
                    _pendingLogEntries.Clear();
                }

                foreach (var pendingLogEntry in buffer)
                {
                    PublishInternal(pendingLogEntry);
                }

                buffer.Clear();
            }
        }
    }
}
