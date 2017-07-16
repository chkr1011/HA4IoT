using System;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Logging
{
    public sealed class LogServicePublisher : ILogger
    {
        private readonly string _source;
        private readonly LogService _logService;

        public LogServicePublisher(string source, LogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _source = source;
        }

        public void Verbose(string message)
        {
            _logService.Publish(LogEntrySeverity.Verbose, _source, message, null);
        }

        public void Info(string message)
        {
            _logService.Publish(LogEntrySeverity.Info, _source, message, null);
        }

        public void Warning(string message)
        {
            _logService.Publish(LogEntrySeverity.Warning, _source, message, null);
        }

        public void Warning(Exception exception, string message)
        {
            _logService.Publish(LogEntrySeverity.Warning, _source, message, exception);
        }

        public void Error(string message)
        {
            _logService.Publish(LogEntrySeverity.Error, _source, message, null);
        }

        public void Error(Exception exception, string message)
        {
            _logService.Publish(LogEntrySeverity.Error, _source, message, exception);
        }

        public void Publish(LogEntrySeverity severity, string message, Exception exception)
        {
            _logService.Publish(severity, _source, message, exception);
        }
    }
}
