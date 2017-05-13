using System;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Logging
{
    public class LogServicePublisher : ILogger
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
            _logService.Verbose(_source, message);
        }

        public void Info(string message)
        {
            _logService.Info(_source, message);
        }

        public void Warning(string message)
        {
            _logService.Warning(_source, message);
        }

        public void Warning(Exception exception, string message)
        {
            _logService.Warning(_source, exception, message);
        }

        public void Error(string message)
        {
            _logService.Error(_source, message);
        }

        public void Error(Exception exception, string message)
        {
            _logService.Error(_source, exception, message);
        }

        public void Publish(LogEntrySeverity severity, string message, Exception exception)
        {
            _logService.Publish(severity, _source, message, exception);
        }
    }
}
