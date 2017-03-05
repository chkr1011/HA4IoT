using System;
using System.Collections.Generic;

namespace HA4IoT.Contracts.Logging
{
    public static class Log
    {
        private static readonly List<ILogger> Adapters = new List<ILogger>();

        public static void RegisterAdapter(ILogger logger)
        {
            lock (Adapters)
            {
                Adapters.Add(logger);
            }
        }

        public static void Error(Exception exception, string message)
        {
            lock (Adapters)
            {
                foreach (var adapter in Adapters)
                {
                    adapter?.Error(exception, message);
                }
            }
        }

        public static void Warning(string message)
        {
            lock (Adapters)
            {
                foreach (var adapter in Adapters)
                {
                    adapter?.Warning(message);
                }
            }
        }

        public static void Warning(Exception exception, string message)
        {
            lock (Adapters)
            {
                foreach (var adapter in Adapters)
                {
                    adapter?.Warning(exception, message);
                }
            }
        }

        public static void Info(string message)
        {
            lock (Adapters)
            {
                foreach (var adapter in Adapters)
                {
                    adapter?.Info(message);
                }
            }
        }

        public static void Verbose(string message)
        {
            lock (Adapters)
            {
                foreach (var adapter in Adapters)
                {
                    adapter?.Verbose(message);
                }
            }
        }
    }
}
