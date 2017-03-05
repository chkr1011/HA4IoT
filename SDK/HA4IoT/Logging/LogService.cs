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
    public class LogService : ILogService
    {
        private const int LogHistorySize = 1000;

        private readonly Guid _sessionId = Guid.NewGuid();
        private readonly Queue<LogEntry> _entries = new Queue<LogEntry>();
        private readonly IDateTimeService _dateTimeService;

        private long _id;

        public LogService(IDateTimeService dateTimeService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;

            Log.RegisterAdapter(CreatePublisher(null));
        }

        public event EventHandler<LogEntryPublishedEventArgs> LogEntryPublished;

        public void Startup()
        {
        }

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
            Add(LogEntrySeverity.Warning, source, message, null);
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
            lock (_entries)
            {
                if (request.SessionId != _sessionId)
                {
                    logEntries = _entries.Take(request.MaxCount).ToList();
                }
                else
                {
                    logEntries = _entries.Where(e => e.Id > request.Offset).Take(request.MaxCount).ToList();
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
            lock (_entries)
            {
                _id += 1L;
                logEntry = new LogEntry(_id, _dateTimeService.Now, Environment.CurrentManagedThreadId, severity, source, message, exception?.ToString());

                _entries.Enqueue(logEntry);
                while (_entries.Count > LogHistorySize)
                {
                    _entries.Dequeue();
                }
            }

            LogEntryPublished?.Invoke(this, new LogEntryPublishedEventArgs(logEntry));

            if (!Debugger.IsAttached)
            {
                return;
            }
            
            Debug.WriteLine($"{severity}@{source}: {message}");
        }
    }
}
