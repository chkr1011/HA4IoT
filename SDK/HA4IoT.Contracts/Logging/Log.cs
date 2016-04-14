using System;

namespace HA4IoT.Contracts.Logging
{
    public static class Log
    {
        public static event EventHandler ErrorLogged;

        public static ILogger Instance { get; set; }
        
        public static void Error(Exception exception, string message)
        {
            Instance?.Error(exception, message);
            ErrorLogged?.Invoke(null, new EventArgs());
        }

        public static void Info(string message)
        {
            Instance?.Info(message);
        }

        public static void Verbose(string message)
        {
            Instance?.Verbose(message);
        }

        public static void Warning(string message)
        {
            Instance?.Warning(message);
        }

        public static void Warning(Exception exception, string message)
        {
            Instance?.Warning(exception, message);
        }
    }
}
