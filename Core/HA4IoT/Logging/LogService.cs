using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Logging
{
    [ApiServiceClass(typeof(ILogService))]
    public class LogService : ServiceBase, ILogService
    {
        private const int LogHistorySize = 1000;
        private const int LogErrorHistorySize = 250;
        private const int LogWarningHistorySize = 250;

        private readonly Guid _sessionId = Guid.NewGuid();

        private readonly object _syncRoot = new object();
        private readonly Queue<LogEntry> _logEntries = new Queue<LogEntry>();
        private readonly Queue<LogEntry> _errorLogEntries = new Queue<LogEntry>();
        private readonly Queue<LogEntry> _warningLogEntries = new Queue<LogEntry>();

        private readonly IDateTimeService _dateTimeService;

        private long _id;

        public LogService(IDateTimeService dateTimeService, ISystemInformationService systemInformationService)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));

            Log.Default = CreatePublisher(null);

            systemInformationService.Set("Log/Errors/Count", () => _errorLogEntries.Count);
            systemInformationService.Set("Log/Warnings/Count", () => _warningLogEntries.Count);
        }

        public event EventHandler<LogEntryPublishedEventArgs> LogEntryPublished;

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
            Add(LogEntrySeverity.Verbose, source, message, null);
        }

        public void Info(string message)
        {
            Info(null, message);
        }

        public void Info(string source, string message)
        {
            Add(LogEntrySeverity.Info, source, message, null);
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
            Add(LogEntrySeverity.Warning, source, message, exception);
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
            Add(LogEntrySeverity.Error, source, message, exception);
        }

        [ApiMethod]
        public void ResetWarningLogEntries(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                _warningLogEntries.Clear();
            }
        }

        [ApiMethod]
        public void GetWarningLogEntries(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                CreateGetLogEntriesResponse(_warningLogEntries, apiContext);
            }
        }

        [ApiMethod]
        public void ResetErrorLogEntries(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                _errorLogEntries.Clear();
            }
        }

        [ApiMethod]
        public void GetErrorLogEntries(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                CreateGetLogEntriesResponse(_errorLogEntries, apiContext);
            }
        }

        [ApiMethod]
        public void GetLogEntries(IApiContext apiContext)
        {
            var request = apiContext.Parameter.ToObject<GetLogEntriesRequest>();
            if (request == null)
            {
                apiContext.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            if (request.MaxCount == 0)
            {
                request.MaxCount = 100;
            }

            List<LogEntry> logEntries;
            lock (_syncRoot)
            {
                if (request.SessionId != _sessionId)
                {
                    logEntries = _logEntries.Take(request.MaxCount).ToList();
                }
                else
                {
                    logEntries = _logEntries.Where(e => e.Id > request.Offset).Take(request.MaxCount).ToList();
                }
            }

            var response = new GetLogEntriesResponse
            {
                SessionId = _sessionId,
                LogEntries = logEntries
            };

            apiContext.Result = JObject.FromObject(response);
        }

        private void Add(LogEntrySeverity severity, string source, string message, Exception exception)
        {
            LogEntry logEntry;
            lock (_syncRoot)
            {
                _id += 1L;
                logEntry = new LogEntry(_id, _dateTimeService.Now, Environment.CurrentManagedThreadId, severity, source, message, exception?.ToString());

                EnqueueLogEntry(logEntry, _logEntries, LogHistorySize);

                if (severity == LogEntrySeverity.Warning)
                {
                    EnqueueLogEntry(logEntry, _warningLogEntries, LogWarningHistorySize);
                }
                else if (severity == LogEntrySeverity.Error)
                {
                    EnqueueLogEntry(logEntry, _errorLogEntries, LogErrorHistorySize);
                }
            }

            var eventArgs = new LogEntryPublishedEventArgs(logEntry);
            LogEntryPublished?.Invoke(this, eventArgs);
            Log.ForwardPublishedLogEntry(eventArgs);

            if (!Debugger.IsAttached)
            {
                return;
            }
            
            Debug.WriteLine($"[{logEntry.Severity}] [{logEntry.Source}] [{logEntry.ThreadId}]: {message}");
        }

        private void EnqueueLogEntry(LogEntry logEntry, Queue<LogEntry> target, int maxLogEntriesCount)
        {
            target.Enqueue(logEntry);

            while (target.Count > maxLogEntriesCount)
            {
                _logEntries.Dequeue();
            }
        }

        private void CreateGetLogEntriesResponse(Queue<LogEntry> source, IApiContext apiContext)
        {
            var response = new GetLogEntriesResponse
            {
                SessionId = _sessionId,
                LogEntries = source.ToList()
            };

            apiContext.Result = JObject.FromObject(response);
        }
    }
}
