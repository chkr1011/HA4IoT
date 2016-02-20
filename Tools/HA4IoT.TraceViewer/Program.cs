using System;
using System.IO;
using HA4IoT.TraceViewer.Properties;

namespace HA4IoT.TraceViewer
{
    public static class Program
    {
        private static readonly object SyncRoot = new object();
        private static readonly TraceReceiver TraceItemReceiver = new TraceReceiver();

        private static bool _loggingIsEnabled;

        public static void Main()
        {
            Console.Title = "Trace Viewer - HA4IoT";

            Console.Write("Starting... ");

            PrepareLogging();
            TraceItemReceiver.TraceItemReceived += PrintTraceItem;
            TraceItemReceiver.Start();

            Console.WriteLine("[OK]");
            Console.WriteLine("- Press 'C' to clear the console");
            Console.WriteLine("- Press 'D' to delete the log file (if logging activated)");
            Console.WriteLine("- Press 'Q' to quit");
            
            ConsoleKeyInfo pressedKey;
            do
            {
                pressedKey = Console.ReadKey(true);
                if (pressedKey.Key == ConsoleKey.C)
                {
                    lock (SyncRoot)
                    {
                        Console.Clear();
                    }
                }
                else if (pressedKey.Key == ConsoleKey.D)
                {
                    lock (SyncRoot)
                    {
                        if (_loggingIsEnabled)
                        {
                            File.Delete(Settings.Default.LogFilename);
                            Console.WriteLine($"Log file '{Settings.Default.LogFilename}' deleted");
                        }
                    }
                }
            } while (pressedKey.Key != ConsoleKey.Q);
        }

        private static void PrepareLogging()
        {
            if (!Settings.Default.ClearLogOnStartup)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(Settings.Default.LogFilename))
            {
                _loggingIsEnabled = true;

                if (File.Exists(Settings.Default.LogFilename))
                {
                    File.Delete(Settings.Default.LogFilename);
                }
            }
        }

        private static void PrintTraceItem(object sender, TraceItemReceivedEventArgs e)
        {
            string timestamp = e.TraceItem.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var line = $"[{e.SenderAddress}] [{timestamp}] [{e.TraceItem.ThreadId}] [{e.TraceItem.Type}]: {e.TraceItem.Message}";

            var color = ConsoleColor.White;
            switch (e.TraceItem.Type)
            {
                case TraceItemSeverity.Verbose:
                {
                    color = ConsoleColor.Gray;
                    break;
                }

                case TraceItemSeverity.Info:
                {
                    color = ConsoleColor.Green;
                    break;
                }

                case TraceItemSeverity.Warning:
                {
                    color = ConsoleColor.Yellow;
                    break;
                }

                case TraceItemSeverity.Error:
                {
                    color = ConsoleColor.Red;
                    break;
                }
            }
            
            lock (SyncRoot)
            {
                if (_loggingIsEnabled)
                {
                    File.AppendAllText(Settings.Default.LogFilename, line + Environment.NewLine);
                }

                Console.ForegroundColor = color;
                Console.WriteLine(line);
            }
        }
    }
}