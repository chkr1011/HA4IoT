using System;
using System.Diagnostics;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Tests.Mockups
{
    public class TestLogger : ILogger
    {
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
            Publish(LogEntrySeverity.Warning, message + "\r\n" + exception.Message);
        }

        public void Error(string message)
        {
            Publish(LogEntrySeverity.Error, message);
        }

        public void Error(Exception exception, string message)
        {
            Publish(LogEntrySeverity.Error, message + "\r\n" + exception.Message);
        }

        public void Verbose(string message)
        {
            Publish(LogEntrySeverity.Verbose, message);
        }

        public void Publish(LogEntrySeverity severity, string message, Exception exception = null)
        {
            Debug.WriteLine(severity + ": " + message);
        }
    }
}
