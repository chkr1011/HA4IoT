using System;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Logging
{
    public class LogServicePublisher : ILogger
    {
        private readonly LogService _logService;

        public LogServicePublisher(string source, LogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            Source = source;
        }

        public string Source { get; }

        public void Verbose(string message)
        {
            _logService.Verbose(Source, message);
        }

        public void Info(string message)
        {
            _logService.Info(Source, message);
        }

        public void Warning(string message)
        {
            _logService.Warning(Source, message);
        }

        public void Warning(Exception exception, string message)
        {
            _logService.Warning(Source, exception, message);
        }

        public void Error(string message)
        {
            _logService.Error(Source, message);
        }

        public void Error(Exception exception, string message)
        {
            _logService.Error(Source, exception, message);
        }
    }
}
