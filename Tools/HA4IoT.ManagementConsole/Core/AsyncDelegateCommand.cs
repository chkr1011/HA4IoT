using System;
using System.Threading.Tasks;
using System.Windows.Input;
using HA4IoT.ManagementConsole.Properties;

namespace HA4IoT.ManagementConsole.Core
{
    public class AsyncDelegateCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public AsyncDelegateCommand([NotNull] Func<Task> execute, [CanBeNull] Func<bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            _execute = execute;

            if (canExecute != null)
            {
                _canExecute = canExecute;
            }
        }

        public event EventHandler CanExecuteChanged;

        public void CheckCanExecute()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public async void Execute(object parameter)
        {
            await _execute();
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute();
        }
    }
}
