using System;
using System.Windows.Input;
using HA4IoT.ManagementConsole.Properties;

namespace HA4IoT.ManagementConsole.Core
{
    public class DelegateCommand : ICommand, ICheckCanExecute
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public DelegateCommand([NotNull] Action execute, [CanBeNull] Func<bool> canExecute = null)
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

        public void Execute(object parameter)
        {
            _execute();
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
