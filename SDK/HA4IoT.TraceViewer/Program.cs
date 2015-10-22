using System;

namespace HA4IoT.TraceViewer
{
    public static class Program
    {
        private static readonly object ConsoleSyncRoot = new object();
        private static readonly ControllerNotificationReceiver NotificationReceiver = new ControllerNotificationReceiver();

        public static void Main(string[] args)
        {
            WriteConsoleOutput(ConsoleColor.Green, "Application started.\r\n- Press 'c' to clear the screen\r\n- Press 'q' to quit.");
            NotificationReceiver.NotificationReceived += HandleNotification;
            NotificationReceiver.Start();

            ConsoleKeyInfo pressedKey;
            do
            {
                pressedKey = Console.ReadKey(true);
                if (pressedKey.Key == ConsoleKey.C)
                {
                    lock (ConsoleSyncRoot)
                    {
                        Console.Clear();
                    }
                }
            } while (pressedKey.Key != ConsoleKey.Q);
        }

        private static void HandleNotification(object sender, ControllerNotificationReceivedEventArguments e)
        {
            var line = e.Notification.RemoteAddress + " " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + ": " +
                       e.Notification.Message;

            var color = ConsoleColor.White;
            switch (e.Notification.Type)
            {
                case NotificationType.Verbose:
                {
                    color = ConsoleColor.Gray;
                    break;
                }

                case NotificationType.Info:
                {
                    color = ConsoleColor.Green;
                    break;
                }

                case NotificationType.Warning:
                {
                    color = ConsoleColor.DarkYellow;
                    break;
                }

                case NotificationType.Error:
                {
                    color = ConsoleColor.Red;
                    break;
                }
            }

            lock (ConsoleSyncRoot)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(line);
            }
        }

        private static void WriteConsoleOutput(ConsoleColor color, string message, params object[] arguments)
        {
            var line = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + ": " + string.Format(message, arguments);

            lock (ConsoleSyncRoot)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(line);
            }
        }
    }
}