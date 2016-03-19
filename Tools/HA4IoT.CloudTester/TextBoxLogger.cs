using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.CloudTester
{
    public class TextBoxLogger : ILogger
    {
        private readonly TextBox _target;

        public TextBoxLogger(TextBox target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            _target = target;
        }

        public void Verbose(string message, params object[] parameters)
        {
            AppendText("VERBOSE: " + message, parameters);
        }

        public void Info(string message, params object[] parameters)
        {
            AppendText("INFORMATION: " + message, parameters);
        }

        public void Warning(string message, params object[] parameters)
        {
            AppendText("WARNING: " + message, parameters);
        }

        public void Warning(Exception exception, string message, params object[] parameters)
        {
            AppendText("WARNING: " + message + Environment.NewLine + exception, parameters);
        }

        public void Error(string message, params object[] parameters)
        {
            AppendText("ERROR: " + message, parameters);
        }

        public void Error(Exception exception, string message, params object[] parameters)
        {
            AppendText("ERROR: " + message + Environment.NewLine + exception, parameters);
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
