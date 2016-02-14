using System;

namespace HA4IoT.Contracts.Logging
{
    public interface ILogger
    {
        void Verbose(string message, params object[] parameters);

        void Info(string message, params object[] parameters);

        void Warning(string message, params object[] parameters);

        void Warning(Exception exception, string message, params object[] parameters);

        void Error(string message, params object[] parameters);

        void Error(Exception exception, string message, params object[] parameters);
    }
}
