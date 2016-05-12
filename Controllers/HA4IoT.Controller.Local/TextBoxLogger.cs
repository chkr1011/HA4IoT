using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Controller.Local
{
    public class TextBoxLogger : ILogger
    {
        private readonly TextBox _target;

        public TextBoxLogger(TextBox target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            _target = target;
        }

        public void Verbose(string message)
        {
            AppendText("VERBOSE: " + message);
        }

        public void Info(string message)
        {
            AppendText("INFORMATION: " + message);
        }

        public void Warning(string message)
        {
            AppendText("WARNING: " + message);
        }

        public void Warning(Exception exception, string message)
        {
            AppendText("WARNING: " + message + Environment.NewLine + exception);
        }

        public void Error(string message)
        {
            AppendText("ERROR: " + message);
        }

        public void Error(Exception exception, string message)
        {
            AppendText("ERROR: " + message + Environment.NewLine + exception);
        }

        private void AppendText(string format, params object[] parameters)
        {
            string message = $"{DateTime.Now.ToString("HH:mm:ss.ffff")}: ";
            if (parameters.Length > 0)
            {
                message += string.Format(format, parameters);
            }
            else
            {
                message += format;
            }
        
            _target.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, 
                () =>
                {
                    _target.Text += message + Environment.NewLine;
                }).AsTask().Wait();
        }
    }
}
