using System;

namespace HA4IoT.Contracts.Logging
{
    public static class Log
    {
        public static event EventHandler ErrorLogged;

        public static ILogger Instance { get; set; }
        
        public static void Error(Exception exception, string message, params object[] parameters)
        {
            Instance?.Error(exception, message, parameters);
            ErrorLogged?.Invoke(null, new EventArgs());
        }

        public static void Info(string message, params object[] parameters)
        {
            Instance?.Info(message, parameters);
        }

        public static void Verbose(string message, params object[] parameters)
        {
            Instance?.Verbose(message, parameters);
        }

        public static void Warning(string message, params object[] parameters)
        {
            Instance?.Warning(message, parameters);
        }

        public static void Warning(Exception exception, string message, params object[] parameters)
        {
            Instance?.Warning(exception, message, parameters);
        }
    }
}
