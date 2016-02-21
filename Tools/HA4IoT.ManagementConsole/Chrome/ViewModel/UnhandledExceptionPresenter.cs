using System;
using System.Windows;
using System.Windows.Input;
using HA4IoT.ManagementConsole.Core;

namespace HA4IoT.ManagementConsole.Chrome.ViewModel
{
    public class UnhandledExceptionPresenter : ViewModelBase
    {
        public UnhandledExceptionPresenter()
        {
            CloseCommand = new DelegateCommand(Close);
            CopyStackTraceCommand = new DelegateCommand(CopyStackTrace);
        }

        public ICommand CloseCommand { get; private set; }

        public ICommand CopyStackTraceCommand { get; private set; }

        public string Message { get; private set; }

        public string StackTrace { get; private set; }

        public bool IsShowingException { get; private set; }

        public void Show(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Show(exception)));
                return;
            }

            Message = exception.Message;
            StackTrace = exception.StackTrace;
            IsShowingException = true;

            OnPropertyChanged(string.Empty);
        }

        private void Close()
        {
            IsShowingException = false;

            OnPropertyChanged(string.Empty);
        }

        private void CopyStackTrace()
        {
            Clipboard.SetText(StackTrace);
        }
    }
}
